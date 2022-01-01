#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NINA;

namespace StarDataImport {

    internal class Program {

        public class DatabaseInteraction {
            private string _connectionString = @"Data Source=" + AppDomain.CurrentDomain.BaseDirectory + @"\Database\NINA.sqlite;foreign keys=true;";

            public DatabaseInteraction() {
                _connection = new SQLiteConnection(_connectionString);
            }

            public DatabaseInteraction(string connection) {
                _connection = new SQLiteConnection(connection);
            }

            private SQLiteConnection _connection;

            public int GenericQuery(string query) {
                _connection.Open();

                SQLiteCommand command = new SQLiteCommand(query, _connection);
                var rows = command.ExecuteNonQuery();
                _connection.Close();

                return rows;
            }

            public void CreateDatabase() {
                var dir = AppDomain.CurrentDomain.BaseDirectory + @"\Database";
                var dbfile = dir + @"\NINA.sqlite";
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(dbfile)) {
                    SQLiteConnection.CreateFile(dbfile);
                }
            }

            public void BulkInsert(ICollection<string> queries) {
                _connection.Open();
                using (SQLiteCommand cmd = _connection.CreateCommand()) {
                    using (var transaction = _connection.BeginTransaction()) {
                        foreach (var q in queries) {
                            cmd.CommandText = q;
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
                _connection.Close();
            }
        }

        private static void Main(string[] args) {
            UpdateEarthRotationParameters();

            //ImportLongTermEarthOrientationDataParameters();

            //ImportFromRapidDataAndPredictionEarthOrientationParameters();

            //UpdateDSONamesFromLocalStore(@"D:\Projects\StarDataXML\");
            //UpdateDSODetailsFromLocalStore(@"D:\Projects\StarDataXML\");

            //DownloadAndStoreSoapStarData();
            //GenerateStarDatabase();
            //UpdateDSOCatalogueWithSpokenNames();
            //UpdateBrightStars();
            //GenerateDatabase();
            //UpdateStarData();
        }

        public static async Task UpdateEarthRotationParameters() {
            var path = Environment.ExpandEnvironmentVariables(@"%localappdata%\nina\NINA.sqlite");
            var connectionString = string.Format(@"Data Source={0};foreign keys=true;", path);
            var db = new DatabaseInteraction(connectionString);

            double availableDataTimeStamp = double.MinValue;
            using (var context = new NINA.Core.Database.NINADbContext(connectionString)) {
                availableDataTimeStamp = (await context.EarthRotationParameterSet.Where(x => x.lod > 0).OrderByDescending(x => x.date).FirstAsync()).date;
            }

            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "N.I.N.A. Data Import");
            webClient.Headers.Add("Accept", "*/*");
            webClient.Headers.Add("Cache-Control", "no-cache");
            webClient.Headers.Add("Host", "datacenter.iers.org");
            webClient.Headers.Add("accept-encoding", "gzip,deflate");

            var data = webClient.DownloadString("https://datacenter.iers.org/data/csv/finals2000A.daily.csv");

            List<string> queries = new List<string>();

            using (var reader = new System.IO.StringReader(data)) {
                string headerLine = reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null) {
                    var columns = line.Split(';');

                    //When column 5 is empty there is no prediction available
                    if (!string.IsNullOrWhiteSpace(columns[5])) {
                        int year = int.Parse(columns[1]);
                        int month = int.Parse(columns[2]);
                        int day = int.Parse(columns[3]);

                        var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
                        var unixTimestamp = (int)(date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                        if (unixTimestamp >= availableDataTimeStamp) {
                            double mjd = double.Parse(columns[0], CultureInfo.InvariantCulture);

                            double x = double.Parse(columns[5], CultureInfo.InvariantCulture);
                            double y = double.Parse(columns[7], CultureInfo.InvariantCulture);

                            double ut1_utc = double.Parse(columns[10], CultureInfo.InvariantCulture);

                            double LOD = 0.0;
                            if (!string.IsNullOrWhiteSpace(columns[12])) {
                                LOD = double.Parse(columns[12], CultureInfo.InvariantCulture);
                            }

                            double dX = 0.0;
                            double dY = 0.0;
                            if (!string.IsNullOrWhiteSpace(columns[19])) {
                                dX = double.Parse(columns[19], CultureInfo.InvariantCulture) / 1000d;
                                dY = double.Parse(columns[21], CultureInfo.InvariantCulture) / 1000d;
                            }

                            //(date,modifiedjuliandate,x,y,ut1_utc,lod,dx,dy)
                            queries.Add($"({unixTimestamp.ToString(CultureInfo.InvariantCulture)},{mjd.ToString(CultureInfo.InvariantCulture)},{x.ToString(CultureInfo.InvariantCulture)},{y.ToString(CultureInfo.InvariantCulture)},{ut1_utc.ToString(CultureInfo.InvariantCulture)},{LOD.ToString(CultureInfo.InvariantCulture)},{dX.ToString(CultureInfo.InvariantCulture)},{dY.ToString(CultureInfo.InvariantCulture)})");
                        }
                    }
                }
            }
            var importQuery = $"INSERT OR REPLACE INTO `earthrotationparameters` (date,modifiedjuliandate,x,y,ut1_utc,lod,dx,dy) VALUES {string.Join($",{Environment.NewLine}", queries)}";
        }

        public static void ImportFromRapidDataAndPredictionEarthOrientationParameters() {
            var connectionString = string.Format(@"Data Source={0};foreign keys=true;", @"D:\Projects\nina\NINA\Database\NINA.sqlite");
            var db = new DatabaseInteraction(connectionString);

            var lines = File.ReadAllLines(@"finals2000A.data.csv");

            var queries = new List<string>();

            for (int i = 1; i < lines.Length; i++) {
                var line = lines[i];
                var columns = line.Split(';');

                //When column 5 is empty there is no prediction available
                if (!string.IsNullOrWhiteSpace(columns[5])) {
                    int year = int.Parse(columns[1]);
                    int month = int.Parse(columns[2]);
                    int day = int.Parse(columns[3]);

                    var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
                    var unixTimestamp = (int)(date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                    double mjd = double.Parse(columns[0], CultureInfo.InvariantCulture);

                    double x = double.Parse(columns[5], CultureInfo.InvariantCulture);
                    double y = double.Parse(columns[7], CultureInfo.InvariantCulture);

                    double ut1_utc = double.Parse(columns[10], CultureInfo.InvariantCulture);

                    double LOD = 0.0;
                    if (!string.IsNullOrWhiteSpace(columns[12])) {
                        LOD = double.Parse(columns[12], CultureInfo.InvariantCulture);
                    }

                    double dX = 0.0;
                    double dY = 0.0;
                    if (!string.IsNullOrWhiteSpace(columns[19])) {
                        dX = double.Parse(columns[19], CultureInfo.InvariantCulture) / 1000d;
                        dY = double.Parse(columns[21], CultureInfo.InvariantCulture) / 1000d;
                    }

                    queries.Add($"INSERT OR REPLACE INTO earthrotationparameters (date, modifiedjuliandate, x, y, ut1_utc, lod, dx, dy) VALUES ({unixTimestamp},{mjd.ToString(CultureInfo.InvariantCulture)},{x.ToString(CultureInfo.InvariantCulture)},{y.ToString(CultureInfo.InvariantCulture)},{ut1_utc.ToString(CultureInfo.InvariantCulture)},{LOD.ToString(CultureInfo.InvariantCulture)},{dX.ToString(CultureInfo.InvariantCulture)},{dY.ToString(CultureInfo.InvariantCulture)});");
                }
            }
            db.BulkInsert(queries);
        }

        public static void ImportLongTermEarthOrientationDataParameters() {
            var connectionString = string.Format(@"Data Source={0};foreign keys=true;", @"D:\Projects\nina\NINA\Database\NINA.sqlite");
            var db = new DatabaseInteraction(connectionString);
            db.GenericQuery("DROP TABLE IF EXISTS earthrotationparameters");

            db.GenericQuery(@"CREATE TABLE IF NOT EXISTS earthrotationparameters (
                date INT,
                modifiedjuliandate REAL,
                x REAL,
                y REAL,
                ut1_utc REAL,
                lod REAL,
                dx REAL,
                dy REAL,
                PRIMARY KEY (date)
            );");

            var lines = File.ReadAllLines(@"224_EOP_C04_14.62-NOW.IAU2000A224.txt");

            var queries = new List<string>();

            for (int i = 14; i < lines.Length; i++) {
                var line = lines[i];
                var columns = line.Split(' ').Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

                int year = int.Parse(columns[0]);
                int month = int.Parse(columns[1]);
                int day = int.Parse(columns[2]);

                var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
                var unixTimestamp = (int)(date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                double mjd = double.Parse(columns[3], CultureInfo.InvariantCulture);

                double x = double.Parse(columns[4], CultureInfo.InvariantCulture);
                double y = double.Parse(columns[5], CultureInfo.InvariantCulture);

                double ut1_utc = double.Parse(columns[6], CultureInfo.InvariantCulture);

                double LOD = double.Parse(columns[7], CultureInfo.InvariantCulture);

                double dX = double.Parse(columns[8], CultureInfo.InvariantCulture);
                double dY = double.Parse(columns[9], CultureInfo.InvariantCulture);

                queries.Add($"INSERT INTO earthrotationparameters (date, modifiedjuliandate, x, y, ut1_utc, lod, dx, dy) VALUES ({unixTimestamp},{mjd.ToString(CultureInfo.InvariantCulture)},{x.ToString(CultureInfo.InvariantCulture)},{y.ToString(CultureInfo.InvariantCulture)},{ut1_utc.ToString(CultureInfo.InvariantCulture)},{LOD.ToString(CultureInfo.InvariantCulture)},{dX.ToString(CultureInfo.InvariantCulture)},{dY.ToString(CultureInfo.InvariantCulture)});");
            }
            db.BulkInsert(queries);
        }

        public static void UpdateDSODetailsFromLocalStore(string filepath) {
            List<SimpleDSO> objects = new List<SimpleDSO>();
            var connectionString = string.Format(@"Data Source={0};foreign keys=true;", @"D:\Projects\nina\NINA\Database\NINA.sqlite");
            var query = "select dsodetailid from cataloguenr INNER JOIN dsodetail ON dsodetail.id = cataloguenr.dsodetailid WHERE syncedfrom is null group by dsodetailid order by catalogue;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand()) {
                    command.CommandText = query;

                    var reader = command.ExecuteReader();
                    while (reader.Read()) {
                        objects.Add(new SimpleDSO() { id = reader.GetString(0), name = reader.GetString(0) });
                    }
                }
            }

            foreach (var obj in objects) {
                var xml = XElement.Load(filepath + obj.id);

                var resolvername = "Simbad";
                var resolver = xml.Descendants("Resolver").Where((x) => x.Attribute("name").Value.Contains(resolvername)).FirstOrDefault();

                var ra = resolver.Descendants("jradeg").FirstOrDefault()?.Value;
                var dec = resolver.Descendants("jdedeg").FirstOrDefault()?.Value;

                if (ra == null) {
                    resolvername = "NED";
                    resolver = xml.Descendants("Resolver").Where((x) => x.Attribute("name").Value.Contains(resolvername)).FirstOrDefault();

                    ra = resolver.Descendants("jradeg").FirstOrDefault()?.Value;
                    dec = resolver.Descendants("jdedeg").FirstOrDefault()?.Value;
                }

                if (ra == null) {
                    resolvername = "VizieR";
                    resolver = xml.Descendants("Resolver").Where((x) => x.Attribute("name").Value.Contains(resolvername)).FirstOrDefault();

                    ra = resolver.Descendants("jradeg").FirstOrDefault()?.Value;
                    dec = resolver.Descendants("jdedeg").FirstOrDefault()?.Value;
                }

                Console.WriteLine(obj.ToString());
                if (ra == null) {
                    Console.WriteLine($"NO ENTRY for {obj.id}");
                } else {
                    Console.WriteLine($"Found Entry for {obj.id} " + " RA:" + ra + " DEC:" + dec);
                }

                if (ra != null && dec != null) {
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
                        connection.Open();
                        using (SQLiteCommand command = connection.CreateCommand()) {
                            command.CommandText = "UPDATE dsodetail SET ra = $ra, dec = $dec, lastmodified = CURRENT_TIMESTAMP, syncedfrom = '" + resolvername + "' WHERE id = $id;";
                            command.Parameters.AddWithValue("$id", obj.id);
                            command.Parameters.AddWithValue("$ra", ra);
                            command.Parameters.AddWithValue("$dec", dec);

                            var rows = command.ExecuteNonQuery();
                            Console.WriteLine(string.Format("Inserted {0} row(s)", rows));
                        }
                    }
                }
            }
            Console.ReadLine();
        }

        public static void UpdateDSONamesFromLocalStore(string filepath) {
            DirectoryInfo d = new DirectoryInfo(filepath);
            foreach (var file in d.GetFiles("*.*")) {
                var id = file.Name;
                Console.WriteLine($"Processing {id}");
                var xml = XElement.Load(file.FullName);

                var connectionString = string.Format(@"Data Source={0};foreign keys=true;", @"D:\Projects\nina\NINA\Database\NINA.sqlite");
                using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
                    connection.Open();

                    foreach (var resolver in xml.Descendants("Resolver")) {
                        var aliasList = resolver.Descendants("alias");
                        foreach (var alias in aliasList) {
                            if (alias.Value.StartsWith("NAME ")) {
                                var name = alias.Value.Substring(5);

                                var query = $"SELECT dsodetailid, catalogue, designation FROM cataloguenr WHERE dsodetailid = \"{id}\" AND catalogue = \"NAME\" AND lower(designation) = \"{name.ToLower()}\"";

                                var insert = false;
                                using (var command = connection.CreateCommand()) {
                                    command.CommandText = query;

                                    var reader = command.ExecuteReader();
                                    if (!reader.HasRows) {
                                        insert = true;
                                    }
                                }
                                if (insert) {
                                    using (SQLiteCommand insertcommand = connection.CreateCommand()) {
                                        Console.WriteLine($"Confirm INSERTING {name} for {id}");
                                        if (!string.IsNullOrWhiteSpace(Console.ReadLine())) {
                                            insertcommand.CommandText = "INSERT INTO cataloguenr (dsodetailid, catalogue, designation) VALUES ($id, $catalogue, $designation);";
                                            insertcommand.Parameters.AddWithValue("$id", id);
                                            insertcommand.Parameters.AddWithValue("$catalogue", "NAME");
                                            insertcommand.Parameters.AddWithValue("$designation", name);

                                            var rows = insertcommand.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void DownloadAndStoreSoapStarData() {
            List<SimpleDSO> objects = new List<SimpleDSO>();
            var connectionString = string.Format(@"Data Source={0};foreign keys=true;", @"D:\Projects\nina\NINA\Database\NINA.sqlite");
            var query = "select dsodetailid, catalogue, designation  from cataloguenr INNER JOIN dsodetail ON dsodetail.id = cataloguenr.dsodetailid where syncedfrom is null group by dsodetailid order by catalogue;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand()) {
                    command.CommandText = query;

                    var reader = command.ExecuteReader();
                    while (reader.Read()) {
                        objects.Add(new SimpleDSO() { id = reader.GetString(0), name = reader.GetString(1) + " " + reader.GetString(2) });
                    }
                }
            }

            var i = objects.Count;

            ThreadPool.SetMinThreads(20, 20);

            Parallel.ForEach(objects, new ParallelOptions() { MaxDegreeOfParallelism = 20 }, (obj, state, index) => {
                Interlocked.Decrement(ref i);
                Console.WriteLine($"Remaining rows: {i}, Object Name: {obj.name}");
                var _url = "http://cdsws.u-strasbg.fr/axis/services/Sesame";
                var _action = "";

                XmlDocument soapEnvelopeXml = CreateSoapEnvelope(obj.name);
                HttpWebRequest webRequest = CreateWebRequest(_url, _action);
                webRequest.Timeout = -1;
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to do something usefull
                // here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.

                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult)) {
                    var soap = XElement.Load(webResponse.GetResponseStream());

                    var xmlstring = (from c in soap.Descendants("return") select c).FirstOrDefault()?.Value;
                    var xml = XElement.Parse(xmlstring);
                    xml.Save($@"D:\Projects\StarDataXML\1\{obj.id}");
                }
            });

            Console.WriteLine("Finished");
        }

        public static void UpdateDSOCatalogueWithSpokenNames() {
            List<SimpleDSO> objects = new List<SimpleDSO>();
            var connectionString = string.Format(@"Data Source={0};foreign keys=true;", @"D:\Projects\nina\NINA\Database\NINA.sqlite");
            var query = "select dsodetailid, catalogue, designation  from cataloguenr INNER JOIN dsodetail ON dsodetail.id = cataloguenr.dsodetailid where not exists (select * from cataloguenr innercatalogue where innercatalogue.catalogue = 'NAME' and innercatalogue.dsodetailid = dsodetail.id) group by dsodetailid order by catalogue;";

            //var query = "select dsodetailid, catalogue, designation  from cataloguenr INNER JOIN dsodetail ON dsodetail.id = cataloguenr.dsodetailid and dsodetail.id='NGC1976' where not exists (select * from cataloguenr innercatalogue where innercatalogue.catalogue is null and innercatalogue.dsodetailid = dsodetail.id) group by dsodetailid order by catalogue;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand()) {
                    command.CommandText = query;

                    var reader = command.ExecuteReader();
                    while (reader.Read()) {
                        objects.Add(new SimpleDSO() { id = reader.GetString(0), name = reader.GetString(1) + " " + reader.GetString(2) });
                    }
                }
            }

            var sw = Stopwatch.StartNew();
            var i = 0;

            Parallel.ForEach(objects, (obj, state, index) => {
                Interlocked.Increment(ref i);
                Console.WriteLine($"Processed row: {i}, Object Name: {obj.name}");
                var _url = "http://cdsws.u-strasbg.fr/axis/services/Sesame";
                var _action = "";

                XmlDocument soapEnvelopeXml = CreateSoapEnvelope(obj.name);
                HttpWebRequest webRequest = CreateWebRequest(_url, _action);
                webRequest.Timeout = -1;
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to do something usefull
                // here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.

                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult)) {
                    var soap = XElement.Load(webResponse.GetResponseStream());

                    var xmlstring = (from c in soap.Descendants("return") select c).FirstOrDefault()?.Value;
                    var xml = XElement.Parse(xmlstring);
                    var resolvername = "Simbad";
                    var resolver = xml.Descendants("Resolver").Where((x) => x.Attribute("name").Value.Contains(resolvername)).FirstOrDefault();

                    using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
                        connection.Open();

                        var aliasList = resolver.Descendants("alias");
                        foreach (var alias in aliasList) {
                            if (alias.Value.StartsWith("NAME ")) {
                                var name = alias.Value.Substring(5);

                                using (SQLiteCommand command = connection.CreateCommand()) {
                                    command.CommandText = "INSERT INTO cataloguenr (dsodetailid, catalogue, designation) VALUES ($id, $catalogue, $designation);";
                                    command.Parameters.AddWithValue("$id", obj.id);
                                    command.Parameters.AddWithValue("$catalogue", "NAME");
                                    command.Parameters.AddWithValue("$designation", name);

                                    var rows = command.ExecuteNonQuery();
                                    Console.WriteLine(string.Format("Inserted {0} row(s)", rows));
                                }
                            }
                        }
                    }
                }
            });

            Console.WriteLine(sw.Elapsed);

            Console.ReadLine();
        }

        public static void UpdateBrightStars() {
            List<DatabaseStar> objects = new List<DatabaseStar>();
            var connectionString = string.Format(@"Data Source={0};foreign keys=true;", @"D:\Projects\nina\NINA\Database\NINA.sqlite");
            var query = "select name, ra, dec, magnitude FROM brightstars where syncedfrom is null;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand()) {
                    command.CommandText = query;

                    var reader = command.ExecuteReader();
                    while (reader.Read()) {
                        objects.Add(new DatabaseStar() { Name = reader.GetString(0) });
                    }
                }
            }

            var sw = Stopwatch.StartNew();

            Parallel.ForEach(objects, obj => {
                var _url = "http://cdsws.u-strasbg.fr/axis/services/Sesame";
                var _action = "";

                XmlDocument soapEnvelopeXml = CreateSoapEnvelope(obj.Name);
                HttpWebRequest webRequest = CreateWebRequest(_url, _action);
                webRequest.Timeout = -1;
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to do something usefull
                // here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.

                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult)) {
                    var soap = XElement.Load(webResponse.GetResponseStream());

                    var xmlstring = (from c in soap.Descendants("return") select c).FirstOrDefault()?.Value;
                    var xml = XElement.Parse(xmlstring);
                    var resolvername = "Simbad";
                    var resolver = xml.Descendants("Resolver").Where((x) => x.Attribute("name").Value.Contains(resolvername)).FirstOrDefault();

                    var ra = resolver.Descendants("jradeg").FirstOrDefault()?.Value;
                    var dec = resolver.Descendants("jdedeg").FirstOrDefault()?.Value;

                    if (ra == null) {
                        resolvername = "NED";
                        resolver = xml.Descendants("Resolver").Where((x) => x.Attribute("name").Value.Contains(resolvername)).FirstOrDefault();

                        ra = resolver.Descendants("jradeg").FirstOrDefault()?.Value;
                        dec = resolver.Descendants("jdedeg").FirstOrDefault()?.Value;
                    }

                    if (ra == null) {
                        resolvername = "VizieR";
                        resolver = xml.Descendants("Resolver").Where((x) => x.Attribute("name").Value.Contains(resolvername)).FirstOrDefault();

                        ra = resolver.Descendants("jradeg").FirstOrDefault()?.Value;
                        dec = resolver.Descendants("jdedeg").FirstOrDefault()?.Value;
                    }

                    Console.WriteLine(obj.ToString());
                    if (ra == null) {
                        Console.WriteLine("NO ENTRY");
                    } else {
                        Console.WriteLine("Found " + " RA:" + ra + " DEC:" + dec);
                    }

                    if (ra != null && dec != null) {
                        using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
                            connection.Open();
                            using (SQLiteCommand command = connection.CreateCommand()) {
                                command.CommandText = "UPDATE brightstars SET ra = $ra, dec = $dec, syncedfrom = '" + resolvername + "' WHERE name = $name;";
                                command.Parameters.AddWithValue("$name", obj.Name);
                                command.Parameters.AddWithValue("$ra", ra);
                                command.Parameters.AddWithValue("$dec", dec);

                                var rows = command.ExecuteNonQuery();
                                Console.WriteLine(string.Format("Inserted {0} row(s)", rows));
                            }
                        }
                    }
                }
            });

            Console.WriteLine(sw.Elapsed);

            Console.ReadLine();
        }

        public static void UpdateStarData() {
            List<SimpleDSO> objects = new List<SimpleDSO>();
            var connectionString = string.Format(@"Data Source={0};foreign keys=true;", @"D:\Projects\NINA.sqlite");
            var query = "select dsodetailid, catalogue, designation  from cataloguenr INNER JOIN dsodetail ON dsodetail.id = cataloguenr.dsodetailid WHERE syncedfrom is null group by dsodetailid order by catalogue;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand()) {
                    command.CommandText = query;

                    var reader = command.ExecuteReader();
                    while (reader.Read()) {
                        objects.Add(new SimpleDSO() { id = reader.GetString(0), name = reader.GetString(1) + " " + reader.GetString(2) });
                    }
                }
            }

            var sw = Stopwatch.StartNew();

            Parallel.ForEach(objects, obj => {
                var _url = "http://cdsws.u-strasbg.fr/axis/services/Sesame";
                var _action = "";

                XmlDocument soapEnvelopeXml = CreateSoapEnvelope(obj.name);
                HttpWebRequest webRequest = CreateWebRequest(_url, _action);
                webRequest.Timeout = -1;
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to do something usefull
                // here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.

                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult)) {
                    var soap = XElement.Load(webResponse.GetResponseStream());

                    var xmlstring = (from c in soap.Descendants("return") select c).FirstOrDefault()?.Value;
                    var xml = XElement.Parse(xmlstring);
                    var resolvername = "Simbad";
                    var resolver = xml.Descendants("Resolver").Where((x) => x.Attribute("name").Value.Contains(resolvername)).FirstOrDefault();

                    var ra = resolver.Descendants("jradeg").FirstOrDefault()?.Value;
                    var dec = resolver.Descendants("jdedeg").FirstOrDefault()?.Value;

                    if (ra == null) {
                        resolvername = "NED";
                        resolver = xml.Descendants("Resolver").Where((x) => x.Attribute("name").Value.Contains(resolvername)).FirstOrDefault();

                        ra = resolver.Descendants("jradeg").FirstOrDefault()?.Value;
                        dec = resolver.Descendants("jdedeg").FirstOrDefault()?.Value;
                    }

                    if (ra == null) {
                        resolvername = "VizieR";
                        resolver = xml.Descendants("Resolver").Where((x) => x.Attribute("name").Value.Contains(resolvername)).FirstOrDefault();

                        ra = resolver.Descendants("jradeg").FirstOrDefault()?.Value;
                        dec = resolver.Descendants("jdedeg").FirstOrDefault()?.Value;
                    }

                    Console.WriteLine(obj.ToString());
                    if (ra == null) {
                        Console.WriteLine("NO ENTRY");
                    } else {
                        Console.WriteLine("Found " + " RA:" + ra + " DEC:" + dec);
                    }

                    if (ra != null && dec != null) {
                        using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
                            connection.Open();
                            using (SQLiteCommand command = connection.CreateCommand()) {
                                command.CommandText = "UPDATE dsodetail SET ra = $ra, dec = $dec, syncedfrom = '" + resolvername + "' WHERE id = $id;";
                                command.Parameters.AddWithValue("$id", obj.id);
                                command.Parameters.AddWithValue("$ra", ra);
                                command.Parameters.AddWithValue("$dec", dec);

                                var rows = command.ExecuteNonQuery();
                                Console.WriteLine(string.Format("Inserted {0} row(s)", rows));
                            }
                        }
                    }
                }
            });

            /*foreach(var obj in objects) {
                var _url = "http://cdsws.u-strasbg.fr/axis/services/Sesame";
                var _action = "";

                XmlDocument soapEnvelopeXml = CreateSoapEnvelope(obj.name);
                HttpWebRequest webRequest = CreateWebRequest(_url,_action);
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml,webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null,null);

                // suspend this thread until call is complete. You might want to do something usefull
                // here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.

                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult)) {
                    var soap = XElement.Load(webResponse.GetResponseStream());

                    var xmlstring = (from c in soap.Descendants("return") select c).FirstOrDefault()?.Value;
                    var xml = XElement.Parse(xmlstring);
                    var simbad = xml.Descendants("Resolver").Where((x) => x.Attribute("name").Value.Contains("Simbad")).FirstOrDefault();

                    var ra = simbad.Descendants("jradeg").FirstOrDefault()?.Value;
                    var dec = simbad.Descendants("jdedeg").FirstOrDefault()?.Value;

                    Console.WriteLine(obj.ToString());
                    if (ra == null) {
                        Console.WriteLine("NO ENTRY");
                    }
                    else {
                        Console.WriteLine("Found " + " RA:" + ra + " DEC:" + dec);
                    }

                    if(ra != null && dec != null) {
                        using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
                            connection.Open();
                            using (SQLiteCommand command = connection.CreateCommand()) {
                                command.CommandText = "UPDATE dsodetail SET ra = $ra, dec = $dec WHERE id = $id;";
                                command.Parameters.AddWithValue("$id",obj.id);
                                command.Parameters.AddWithValue("$ra",ra);
                                command.Parameters.AddWithValue("$dec",dec);

                                var rows = command.ExecuteNonQuery();
                                Console.WriteLine(string.Format("Inserted {0} row(s)",rows));
                            }
                        }
                    }
                }
            }*/

            Console.WriteLine(sw.Elapsed);

            Console.ReadLine();
        }

        private static HttpWebRequest CreateWebRequest(string url, string action) {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        private static XmlDocument CreateSoapEnvelope(string target) {
            XmlDocument soapEnvelopeDocument = new XmlDocument();

            soapEnvelopeDocument.LoadXml(string.Format(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no"" ?>
                                            <SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:apachesoap=""http://xml.apache.org/xml-soap"" xmlns:impl=""http://cdsws.u-strasbg.fr/axis/services/Sesame"" xmlns:intf=""http://cdsws.u-strasbg.fr/axis/services/Sesame"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:wsdl=""http://schemas.xmlsoap.org/wsdl/"" xmlns:wsdlsoap=""http://schemas.xmlsoap.org/wsdl/soap/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	                                            <SOAP-ENV:Body>
		                                            <mns:SesameXML xmlns:mns=""http://cdsws.u-strasbg.fr/axis/services/Sesame"" SOAP-ENV:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
			                                            <name xsi:type=""xsd:string"">{0}</name>
		                                            </mns:SesameXML>
	                                            </SOAP-ENV:Body>
                                            </SOAP-ENV:Envelope>
            ", target));
            return soapEnvelopeDocument;
        }

        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest) {
            using (Stream stream = webRequest.GetRequestStream()) {
                soapEnvelopeXml.Save(stream);
            }
        }

        private static void GenerateStarDatabase() {
            var db = new DatabaseInteraction();
            db.CreateDatabase();

            db.GenericQuery("DROP TABLE IF EXISTS brightstars");

            db.GenericQuery(@"CREATE TABLE IF NOT EXISTS brightstars (
                name TEXT NOT NULL,
                ra REAL,
                dec REAL,
                magnitude REAL,
                PRIMARY KEY (name)
            );");

            List<string> queries = new List<string>();

            using (TextFieldParser parser = new TextFieldParser(@"brightstars.csv")) {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                HashSet<string> types = new HashSet<string>();
                var isFirst = true;
                List<DatabaseStar> l = new List<DatabaseStar>();
                var i = 1;
                while (!parser.EndOfData) {
                    string[] fields = parser.ReadFields();
                    //Processing row
                    if (isFirst) {
                        isFirst = false;
                        continue;
                    }

                    DatabaseStar dso = new DatabaseStar(fields);

                    queries.Add(dso.getStarQuery());
                }

                db.BulkInsert(queries);

                Console.WriteLine("Done");
                Console.ReadLine();
            }
        }

        public static void GenerateDatabase() {
            var db = new DatabaseInteraction();
            db.CreateDatabase();

            db.GenericQuery("DROP TABLE IF EXISTS visualdescription");
            db.GenericQuery("DROP TABLE IF EXISTS cataloguenr");
            db.GenericQuery("DROP TABLE IF EXISTS dsodetail;");

            db.GenericQuery(@"CREATE TABLE IF NOT EXISTS dsodetail (
                id TEXT NOT NULL,
                ra REAL,
                dec REAL,
                magnitude REAL,
                surfacebrightness REAL,
                sizemin REAL,
                sizemax REAL,
                positionangle REAL,
                nrofstars REAL,
                brighteststar REAL,
                constellation TEXT,
                dsotype TEXT,
                dsoclass TEXT,
                notes TEXT,
                PRIMARY KEY (id)
            );");

            db.GenericQuery(@"CREATE TABLE IF NOT EXISTS visualdescription (
                dsodetailid TEXT,
                description TEXT,
                PRIMARY KEY (dsodetailid, description),
                FOREIGN KEY (dsodetailid) REFERENCES dsodetail (id)
            );");

            db.GenericQuery(@"CREATE TABLE IF NOT EXISTS cataloguenr (
                dsodetailid TEXT,
                catalogue TEXT,
                designation TEXT,
                PRIMARY KEY (dsodetailid, catalogue, designation),
                FOREIGN KEY (dsodetailid) REFERENCES dsodetail (id)
            );");

            List<string> queries = new List<string>();

            using (TextFieldParser parser = new TextFieldParser(@"SAC_DeepSky_ver81_Excel.csv")) {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                HashSet<string> types = new HashSet<string>();
                var isFirst = true;
                List<DatabaseDSO> l = new List<DatabaseDSO>();
                var i = 1;
                while (!parser.EndOfData) {
                    string[] fields = parser.ReadFields();
                    //Processing row
                    if (isFirst) {
                        isFirst = false;
                        continue;
                    }

                    DatabaseDSO dso = new DatabaseDSO(i++, fields);
                    if (dso.cataloguenr.First().catalogue != null) {
                        l.Add(dso);
                    }

                    queries.Add(dso.getDSOQuery());
                    queries.Add(dso.getCatalogueQuery());
                    queries.Add(dso.getVisualDescriptionQuery());
                }

                var duplicates = l.Where(s => s.Id == string.Empty);
            }

            db.BulkInsert(queries);

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private class cataloguenr {
            public string catalogue;
            public string designation;

            public cataloguenr(string field) {
                catalogue = catalogues.Where((x) => field.StartsWith(x)).FirstOrDefault();
                if (catalogue != null) {
                    catalogue = catalogue.Trim();
                    designation = field.Split(new string[] { catalogue }, StringSplitOptions.None)[1].Trim();
                }
            }

            private string[] catalogues = {
                "3C","Archinal", "Abell","ADS","AM","Antalova", "Auner", "Av-Hunter", "AND","Ap","Arp","Bark","Basel","BD","Berk","Be","Biur","Blanco","Bochum","B","Ced","CGCG","Cr", "Coalsack","Czernik","Danks", "DDO","DoDz","Do","Dun","ESO", "Eridanus Cluster","Fein","Frolov","Graham", "Gum","Haffner","Harvard","Hav-Moffat","He","Hogg","Ho","HP","Hu","H","IC","Isk","J","Kemble","King","Kr","K","Latysev","Lg Magellanic Cl", "Le Gentil", "Lac","Loden","LBN","LDN","NPM1G","Lynga","MCG","Me","Mrk","Mel","M1 thru M4","M","New","NGC","Pal","PB","PC","Pismis","PK","RCW","Roslund","Ru","Sa","Sher","Sh","SL","Steph","Stock","Ter","Tombaugh","Ton","Tr","UGC","UGCA","UKS","Upgren","V V","vdB-Ha","vdBH","vdB","Vy", "VY","Waterloo","Winnecke","ZwG"
            };

            public override string ToString() {
                return catalogue + designation;
            }
        }

        public class visualdescription {
            public string description;

            public visualdescription(string field) {
                description = field;
            }
        }

        private class SimpleDSO {
            public string id;
            public string name;

            public override string ToString() {
                return name;
            }
        }

        private class DatabaseStar {

            private static readonly Lazy<ASCOM.Utilities.Util> lazyAscomUtil =
            new Lazy<ASCOM.Utilities.Util>(() => new ASCOM.Utilities.Util());

            private static ASCOM.Utilities.Util AscomUtil { get { return lazyAscomUtil.Value; } }

            //public string obj;
            //public string other;
            public string Name;

            public double RA;
            public double DEC;
            public double magnitude;

            public DatabaseStar() {
            }

            public DatabaseStar(string[] fields) {
                this.Name = fields[0];

                RA = AscomUtil.HMSToDegrees(fields[2]);
                DEC = double.Parse(fields[3], CultureInfo.InvariantCulture);

                magnitude = double.Parse(fields[1], CultureInfo.InvariantCulture);
            }

            public string getStarQuery() {
                return $@"INSERT INTO brightstars
                (name, ra, dec, magnitude)  VALUES
                (""{Name}"",
                {RA.ToString(CultureInfo.InvariantCulture)},
                {DEC.ToString(CultureInfo.InvariantCulture)},
                {magnitude.ToString(CultureInfo.InvariantCulture)}); ";
            }

            /*internal void insert(DatabaseInteraction db) {
                var q = $@"INSERT INTO dsodetail
                (id, ra, dec, magnitude, surfacebrightness,sizemin,sizemax,positionangle,nrofstars,brighteststar,constellation,dsotype,dsoclass,notes)  VALUES
                ({Name},
                {RA.ToString(CultureInfo.InvariantCulture)},
                {DEC.ToString(CultureInfo.InvariantCulture)},
                {magnitude.ToString(CultureInfo.InvariantCulture)},
                {subr.ToString(CultureInfo.InvariantCulture)},
                {size_min?.ToString(CultureInfo.InvariantCulture) ?? "null"},
                {size_max?.ToString(CultureInfo.InvariantCulture) ?? "null"},
                ""{positionangle}"",
                ""{NSTS}"",
                ""{brighteststar}"",
                ""{constellation}"",
                ""{type}"",
                ""{classification}"",
                ""{Notes}"" ); ";
                db.GenericQuery(q);

                q = "";
                foreach (var cat in cataloguenr) {
                    q += $@"INSERT INTO cataloguenr (dsodetailid, catalogue, designation) VALUES ({Name}, ""{cat.catalogue}"", ""{cat.designation}""); ";
                }
                db.GenericQuery(q);

                q = "";
                foreach (var desc in visualdescription) {
                    q += $@"INSERT INTO visualdescription (dsodetailid, description) VALUES ({Name}, ""{desc.description}""); ";
                }
                db.GenericQuery(q);
            }*/
        }

        private class DatabaseDSO {

            private static readonly Lazy<ASCOM.Utilities.Util> lazyAscomUtil =
            new Lazy<ASCOM.Utilities.Util>(() => new ASCOM.Utilities.Util());

            private static ASCOM.Utilities.Util AscomUtil { get { return lazyAscomUtil.Value; } }

            public List<cataloguenr> cataloguenr;

            //public string obj;
            //public string other;
            public string Id;

            public string type;
            public string constellation;
            public double RA;
            public double DEC;
            public double magnitude;
            public double subr;
            public string u2k;
            public string ti;
            public double? size_max;
            public double? size_min;
            public string positionangle;
            public string classification;
            public string NSTS;
            public string brighteststar;
            public string CHM;
            public List<visualdescription> visualdescription;
            public string Notes;

            public override string ToString() {
                var s = "";
                foreach (var cat in cataloguenr) {
                    s += cat.ToString() + "; ";
                }
                return s;
            }

            public DatabaseDSO(int id, string[] fields) {
                cataloguenr = new List<Program.cataloguenr>();
                var ident = new cataloguenr(fields[0]);
                this.Id = ident.ToString();
                if (this.Id == string.Empty) {
                    Debugger.Break();
                }
                cataloguenr.Add(ident);

                foreach (var field in fields[1].Split(';')) {
                    if (field != string.Empty) {
                        var cat = new cataloguenr(field);
                        if (cataloguenr.Any((x) => x.catalogue == cat.catalogue && x.designation == cat.designation)) {
                            continue;
                        }
                        if (cat.catalogue != null) {
                            cataloguenr.Add(cat);
                        } else {
                        }
                    }
                }
                /*cataloguenr.Add(new Program.cataloguenr() { catalogue = fields[0].Split(' ')[0], designation = fields[0].Split(' ')[1] });

                foreach(var field in fields[1].Split(';')) {
                    if(field != string.Empty) {
                        if(field.Split(' ').Length == 1) { continue; }
                        cataloguenr.Add(new Program.cataloguenr() { catalogue = field.Split(' ')[0], designation = field.Split(' ')[1] });
                    }
                }*/

                //other = fields[1];

                type = fields[2];
                constellation = fields[3];

                RA = AscomUtil.HMSToDegrees(fields[4]);
                DEC = AscomUtil.DMSToDegrees(fields[5]);

                magnitude = double.Parse(fields[6], CultureInfo.CreateSpecificCulture("de-DE"));
                subr = double.Parse(fields[7], CultureInfo.CreateSpecificCulture("de-DE"));
                u2k = fields[8];
                ti = fields[9];

                var size = fields[10];
                if (size.Contains("m")) {
                    size_max = double.Parse(size.Replace("m", string.Empty).Trim(), CultureInfo.InvariantCulture) * 60;
                } else if (size.Contains("s")) {
                    size_max = double.Parse(size.Replace("s", string.Empty).Trim(), CultureInfo.InvariantCulture);
                } else {
                    size_max = null;
                }

                size = fields[11];
                if (size.Contains("m")) {
                    size_min = double.Parse(size.Replace("m", string.Empty).Trim(), CultureInfo.InvariantCulture) * 60;
                } else if (size.Contains("s")) {
                    size_min = double.Parse(size.Replace("s", string.Empty).Trim(), CultureInfo.InvariantCulture);
                } else {
                    size_min = null;
                }

                positionangle = fields[12];
                classification = fields[13];
                NSTS = fields[14];
                brighteststar = fields[15];
                CHM = fields[16];

                visualdescription = new List<visualdescription>();
                if (fields[17] != string.Empty) {
                    foreach (var s in fields[17].Split(';')) {
                        visualdescription.Add(new visualdescription(s));
                    }
                }

                Notes = fields[18];
            }

            public string getDSOQuery() {
                return $@"INSERT INTO dsodetail
                (id, ra, dec, magnitude, surfacebrightness,sizemin,sizemax,positionangle,nrofstars,brighteststar,constellation,dsotype,dsoclass,notes)  VALUES
                (""{Id}"",
                {RA.ToString(CultureInfo.InvariantCulture)},
                {DEC.ToString(CultureInfo.InvariantCulture)},
                {magnitude.ToString(CultureInfo.InvariantCulture)},
                {subr.ToString(CultureInfo.InvariantCulture)},
                {size_min?.ToString(CultureInfo.InvariantCulture) ?? "null"},
                {size_max?.ToString(CultureInfo.InvariantCulture) ?? "null"},
                ""{positionangle}"",
                ""{NSTS}"",
                ""{brighteststar}"",
                ""{constellation}"",
                ""{type}"",
                ""{classification}"",
                ""{Notes}"" ); ";
            }

            public string getCatalogueQuery() {
                var q = "";
                foreach (var cat in cataloguenr) {
                    if (cat.catalogue != null && cat.catalogue.Trim() != string.Empty) {
                        q += $@"INSERT INTO cataloguenr (dsodetailid, catalogue, designation) VALUES (""{Id}"", ""{cat.catalogue}"", ""{cat.designation}""); ";
                    }
                }
                return q;
            }

            public string getVisualDescriptionQuery() {
                var q = "";
                foreach (var desc in visualdescription) {
                    if (desc.description != null && desc.description.Trim() != string.Empty) {
                        q += $@"INSERT INTO visualdescription (dsodetailid, description) VALUES (""{Id}"", ""{desc.description}""); ";
                    }
                }
                return q;
            }

            /*internal void insert(DatabaseInteraction db) {
                var q = $@"INSERT INTO dsodetail
                (id, ra, dec, magnitude, surfacebrightness,sizemin,sizemax,positionangle,nrofstars,brighteststar,constellation,dsotype,dsoclass,notes)  VALUES
                ({Name},
                {RA.ToString(CultureInfo.InvariantCulture)},
                {DEC.ToString(CultureInfo.InvariantCulture)},
                {magnitude.ToString(CultureInfo.InvariantCulture)},
                {subr.ToString(CultureInfo.InvariantCulture)},
                {size_min?.ToString(CultureInfo.InvariantCulture) ?? "null"},
                {size_max?.ToString(CultureInfo.InvariantCulture) ?? "null"},
                ""{positionangle}"",
                ""{NSTS}"",
                ""{brighteststar}"",
                ""{constellation}"",
                ""{type}"",
                ""{classification}"",
                ""{Notes}"" ); ";
                db.GenericQuery(q);

                q = "";
                foreach (var cat in cataloguenr) {
                    q += $@"INSERT INTO cataloguenr (dsodetailid, catalogue, designation) VALUES ({Name}, ""{cat.catalogue}"", ""{cat.designation}""); ";
                }
                db.GenericQuery(q);

                q = "";
                foreach (var desc in visualdescription) {
                    q += $@"INSERT INTO visualdescription (dsodetailid, description) VALUES ({Name}, ""{desc.description}""); ";
                }
                db.GenericQuery(q);
            }*/
        }
    }
}