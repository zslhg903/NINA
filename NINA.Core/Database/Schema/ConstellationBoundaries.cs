#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Data.Entity.ModelConfiguration;

namespace NINA.Core.Database.Schema {

    public class ConstellationBoundaries {
        public string constellation { get; set; }

        public double position { get; set; }
        public double ra { get; set; }
        public double dec { get; set; }
    }

    internal class ConstellationBoundariesConfiguration : EntityTypeConfiguration<ConstellationBoundaries> {

        public ConstellationBoundariesConfiguration() {
            ToTable("dbo.constellationboundaries");
            HasKey(x => new { x.constellation, x.position });
            Property(x => x.constellation).HasColumnName("constellation").IsRequired();
            Property(x => x.position).HasColumnName("position").IsRequired();
            Property(x => x.ra).HasColumnName("ra").IsRequired();
            Property(x => x.dec).HasColumnName("dec").IsRequired();
        }
    }
}