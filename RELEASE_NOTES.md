﻿# N.I.N.A. - Nighttime Imaging 'N' Astronomy Changelog

If N.I.N.A. helps you in your journey for amazing deep sky images, please consider a donation. Each backer will help keeping the project alive and active.  
More details at <a href="https://nighttime-imaging.eu/donate/" target="_blank">nighttime-imaging.eu/donate/</a>

### <span style="color:yellow;">Beta builds are preview builds that contain the full development effort for the next release. These builds contain the full set of features for the next version and are under evaluation to find and fix potential bugs. No major changes will occur in these builds and the focus is on bug fixing only - major changes may only occur if a critical issue is identified and a major change is necessary to fix it. Thus these builds should already be quite stable to use.</br>To be able to roll back to a previous released version without losing the profiles, backup the profiles which are located at %localappdata%\NINA</span>

# Version 2.0 - Beta

The Release Notes below will only mention the changes compared to the last released version.  
To identify what has changed in between beta builds, please refer to the [bitbucket commit history](https://bitbucket.org/Isbeorn/nina/commits/branch/release/2.0)

## Change of existing settings and panels

- Options page tab location is moved to the left instead of the top to be similar to main tabs
- Sequencer end options have been moved into the simple sequencer tab
- Meridian Flip settings show no longer an enabled flag, but must be enabled in the simple sequencer screen or added as a trigger into the advanced sequencer instead
- Autofocus options have been moved into a separate tab
- PHD2 specific settings are now available in the equipment guider tab after connection
- Optimal exposure calculator is removed from the core application, but available as a plugin

## Complete Sequencer Rework

### Sequence Tab changed to show a sequence navigation first
- Old Sequencer has been renamed to "Simple Sequencer"
- The internal engine for the simple sequencer has been completely revamped. The simple sequencer will use the advanced sequencer framework internally while exposing a similar user interface that was used previously
- If required a simple sequence can be generated into the advanced sequence for fine tuning
- Instead of loading an empty target by default, the UI will show a separate UI when no target was set, where the user can choose which kind of sequence should be loaded instead or jump directly into the advanced sequencer
- End of sequence options are moved from the options tab to the sequence tab
- Introduced new start of sequence options to be used
- A new toggle to "Rotate Target" to the desired orientation has been added. Previously this was always done when a rotator was connected and skipped when it was not connected. The new approach should be much more intuitive for the user to understand when rotation will be considered or not.
- Removed external command and flat panel items from the simple sequencer UI, as these are only for advanced usage and not used by the majority that use the simple sequencer
- Added support for fetching sky view and location coordinates from the [C2A](http://www.astrosurf.com/c2a/) and [SkytechX](http://www.skytechx.eu/) planetarium programs
- Sequence mode loop has been changed sligthly to not define a number of exposures on each row but rather on the whole sequence instead to loop through.

### New Advanced Sequencer
- In this area a custom sequence can be built completely from scratch by individual small building blocks
- Each user has a different flow of how a sequence should look like. This advanced planning can serve most requirements that a user might have.
- The building blocks consist of these categories
  - *Instruction Sets*
    - These sets group together different instructions
    - Templates can be generated out of these sets to be used later with the same parameters
  - *Instructions*
    - These are the individual steps to automate the sequence
    - Each item has a different operation, like switching to a filter or taking an exposure 
  - *Trigger*
    - Triggers are part of an instruction set and are evaluated after each instruction is processed
    - They have certain conditions that are checked and when these are fullfilled the triggers will fire
    - Examples for these are autofocus after certain parameters, merdian flip or dithering after an amount of exposures
    - Triggers are always cascading. So a trigger on an instruction set on a higher level than the currently executed instruction set will also be evaluated
  - *Loop Conditions*
    - These conditions are attached to an instruction set and drive the behavior of the set
    - As long as a condition is fullfilled, the instruction set will be looped
    - Once a condition is not fullfilled anymore, all remaining instructions inside an instruction set will be skipped
    - Loop conditions are always cascading. So a conditions on an instruction set on a higher level than the currently executed instruction set will also be evaluated
  - *Templates*
    - Each instruction set can be saved as a template
    - These templates store all info that has been set inside the instruction set and can be restored for later usage
    - Furthermore these will be stored physically inside the default sequence folder and can be reorganized there
    - Sub folders inside the default sequence folder are possible and will group these templates together
  - *Targets*
    - Each Deep Sky Object Sequence be saved as a target
    - These target store all info that has been set inside the deep sky object sequence and can be restored for later usage
    - Furthermore these will be stored physically inside the default sequence folder and can be reorganized there
- For more details on the usage refer to the documentation

## New Plugin Tab
- With the introduction of the advanced sequencer, a sequence can be planned with individual building blocks. These blocks have been designed to work like small plugins that will be initialized on startup. This opens up the possibility for specialized custom plugins that can be installed independently on demand.
- To manage these custom plugins for the advanced sequencer a new application tab page has been introduced. There a user can setup and see all installed plugins as well as having the possiblity to see, install and update plugins from the official online plugin repository.
- Plugins can currently hook into the advanced sequencer, add new dock panels in the imaging tab or add different behaviors for autofocus, star detection and star annotation. More areas to be pluggable are planned for the future.
- The main benefit of these plugins are the possibility to create very specialized behavior, that would only benefit by a smaller user base, without cluttering the application with these capabilities for users that do not need this special behavior.

## Framing Tab
- Instead of sending the current target to the sequencer, the user will be prompted to either choose the simple sequencer or directly send the target to the new advanced sequencer while also being able to choose from different templates
- Replacing of the complete targets is removed, as this is not necessary. 
- Possibility to manually enter target rotation 
- A new multi action button replaces the slew button. This button can either "slew", "slew and center" or "slew, center and rotate" your current framing
- Improved precision when dragging the rectangle around, especially for longer distances
- Added a new option to preserve the alignment when being far away from celestial equator where panels won't be perfectly aligned to a rectangle anymore when having the same rotation
- Visually show the misalignment when having the same rotation for each panel when being further away from the celestial equator
- Added a new grid showing the mosaic panel coordinates and orientations
- Added a center dot for the framing rectangle
- A new toggle is available to toggle the sky background instead of the framing rectangle
- The Sky Object Annotation will now properly draw elliptical objects when position angle and size information is available instead of always showing them as circular

## Imaging Tab - Sequence Panel
- As the new sequencer has a dynamic operation mode, the old summary is not feasible anymore when using the advanced sequencer
- Instead the sequence panel will show a minimized representation of the advanced sequence, where you can see the instructions with basic details
- When using the simple sequencer the old style will still be shown instead

## New hardware support

### MGEN3
- Full control of the MGEN3 by mirroring the controller display into N.I.N.A.'s user interface
- Automatic power-on when connecting
- Starts guiding on sequence start
- Performs calibration and star auto-selection when required
- Displays star drift during guiding in a graph
- Dithering during sequencing

### MetaGuide
- MetaGuide is now available as an option for a guiding application

### SVBony
- Added a native driver for SVBony cameras. Currently without temperature control and tested with SVBony 305M Pro.

### SBIG
- Added a native driver for SBIG cameras.

### MallinCam
- Added native driver for MallinCam cameras.

### Risingcam
- Native support for Risingcam added

### Player One
- Native support for Player One Cameras

### ASCOM Dome
- ASCOM Domes are supported throughout the application
  - Natively provides azimuth synchronization with the telescope, so no additional applications are needed (such as ASCOM Device Hub)
  - Lateral offsets supported, enabling side-by-side telescope setups
  - When synchronization is enabled, telescope slews wait for the dome to synchronize before next actions, such as imaging and plate solving
  - Homes the dome prior to parking, which can improve the reliability of arriving precisely at the park location. This can be important if a shutter motor battery charges in the park position
 - Dome actions provided in the new sequencer
  - Enable dome synchronization
  - Open/Close shutter
  - Park dome  

### ASCOM CoverCalibrator
- With the ASCOM Platform Version 6.5 a new type of interface was added called "CoverCalibrator". 
- These are basically flat panel devices which will now be available to choose from in the application under the flat panel section

### ASCOM SafetyMonitor
- An ASCOM device to monitor safe conditions for an imaging run
- These devices can be used inside the sequencer to interrupt an imaging run, when they report unsafe conditions
- Option to automatically close the dome shutter immediately when the safety monitor reports unsafe conditions

### Native support for Atik EFW2/3 and internal Filterwheels
- The integrated filter wheels for Atik cameras like the Atik One 9.0 are now natively supported
- This allows usage of native camera drivers for Atik cameras with integrated filter wheels
- The Atik EFW2 and 3 can also be natively connected without using the ASCOM driver

### Improvements for Altair, Touptek, Omegaon, Mallincam and Risingcam native drivers
- Native driver implementation unified for a common interface, as the underlying SDKs are similar
- Additive binning can be turned on
- Fan speed can be controlled when available
- Binning info for these cameras are now properly inserted into Metadata

### PHD2
- Dither will be skipped when not actively guiding or the guide star was lost
- Settle time will now correctly be considered when starting guiding
- Guiding start timeout will not consider calibration time and will also be used when "Guiding start retry" is off
- Guiding start retry will not retry more than three times to prevent an infinite loop
- A ROI percentage can now be set for PHD2 to be considered during guidestar search
- Profiles can now be switched from the list of available PHD2 profiles

## Device-related Improvements
- *Canon*: Automatically send request to increase shutdown time, when camera is about to shutdown
- *Nikon*: Fixed an issue where cancelling an exposure would lead to unexpected bulb exposure times
- Flat Device Brightness is no longer expressed in percentage, but rather in the absolute values the flat panel supports
- Flat Device *trained brightness levels* will be *automatically migrated* from percentage to absolute values *after first flat panel device connection*.
- Filter Wheel List of Filters are now all using the list from the profile instead of having a mix of filters from the ASCOM driver and those from the profile. De-Sync of these lists can't happen anymore.
- Some mount drivers have reversed primary and secondary axis implementation for manual movements. Now a reverse flag is available to manually correct this behavior.
- ASCOM Camera driver can now handle odd sensor width & height for mono sensors. A setting to enable this new behavior is available.
- ASCOM connection and disconnection logic is now unified between all devices to ensure same bahvior
- ASCOM connection that is lost without any raised error will now be tried to be reconnected one time. If an error happens due to that the application will disconnect like before.
- ASCOM get and set methods use a unified logic to ensure same behavior for all devices
- *FLI*: Background flush is now disabled prior to readout to prevent a hung readout
- *MGEN2* now supports unattended guide star selection and calibration, and automatic meridian flips
- Altair, MallinCam, Omegon, RisingCam and ToupTek subsampling is now available

## Application Improvements
### General
- The application distribution is now code signed
- Application window will now remember its placing and state
- Application initialization is utilizing more parallel processing to startup faster.
- Application logs are vastly improved to log a lot more info of what is happening at the various levels. INFO level will also contain a lot more info of what is happening at a high level.
- It is now possible to change the application font in the options
- It is now possible to start, resume and cancel sequences from the preview window
- Options menu has been restructured to adhere to the Equipment layout
- Auto-focus is now its own tab in the Options menu
- Empty gain and offset settings will now always reflect the settings set in Equipment - Camera (valid for imaging, sequence and auto-focus)
- Any active field of view or ocular rotation angle is now imported along with coordinates from Stellarium and TheSky X
- New translation for Czech (Čeština) has been added
- New translation for Norwegian Bokmal (Norsk bokmål) has been added
- New translation for Korean (한국어) has been added
- Can send the sequencer target coordinates to the Framing Wizard
- Improved Meridian Flip reliability by retrying when changing the pier or slewing close to the meridian fails. NINA now provides a warning suggesting to increase the meridian wait time if this hapens
- Guider settings moved to the guider equipment page making them easier to find and exposing only settings relevant for each type of guider
- Connect all button now connects all devices in a sequential order to prevent collisions with com port scanning that could happen
- After hitting connect all button, it will show highlighted and when hit again it will disconnect all devices instead
- Meridian Flip now allows for a time range instead of a single point in time to allow for less time lost
- Some areas where the user interface was clipped on lower resolutions (down to 720p) are now showing all details
- Telescope panel in imaging tab now shows an indicator when the telescope is not unparked
- Telescope Park action will now wait for the scope to reach park position
- Profile chooser on startup can now be re-activated in options again once it is disabled
- Guiding gets stopped before any telescope-moving sequencer command gets executed 
- Gain/Offset fields will now show the default values as a hint text, rather than being populated inside the textbox itself. Having this approach, a user can directly enter specific values without having to clear the default value first.
- The imaging tab layout is now saved per profile, instead of one layout for all profiles
- Most options and values now also display a unit of measure if applicable
- Weather information in equipment tab now shows correct wind direction.
- Various layout improvements and redesign of controls
- Flat Wizard page controls are streamlined with the rest of the application by replacing the sliders with steppers
- When closing a dock panel in the imaging tab and reopening it again, the position is properly restored
- Cooling charts in the camera equipment tab have been merged into one single chart with the history size increased from 100 to 1000
- On web requests the user-agent header is now filled properly
- An autofocus indicator in the HFR history will no longer change the Y-Axis scale
- The automatic roll over to a blind solver can now be switched off in the plate solver tab
- For the first image in an image view control, the image will now be shown as size to fit instead of 1:1 by default
- Profiles can now store an arbitrary description

### Subsampling
- The sub sample button above the image panel has been removed
- In the imaging camera control panel when subsampling is enabled a new control will be visible to adjust the subsampling directly there
- This should be more intuitive to use and does not require a certain order of operations and an active non sub sampled image to set up

### Local Horizon Display
- It is now possible to define a custom horizon to be used and displayed in the altitude charts
- Using these custom horizons will make target planning a lot more convenient when only a portion of the sky is available
- The horizon file consists of a simple mapping of azimuth to altitude values
- The sky atlas has a new entry for altitude filter to filter for the object to be above the horizon for the specified time range
----
```markdown
# Example horizon file content
# A line starting with # is treated as a comment
# The horizon file consists of a pair of azimuth and altitude values
# Azimuth values that are not explicitly defined will interpolate the altitude by using the existing datapoints
# A minimum of two points have to be defined
0 10
100 30
150 35
250 30
300 20
350 15
```
----

### Auto-focus system
- Auto-focus can now have different settings for gain, offset and binning per filter
- When using an autofocus filter with offsets this filter will now also be used for the first and last measurement instead of the filter prior to starting the auto focus
- During an auto-focus run while taking an image, if the download fails it will be automatically retried up to two times to try to recover
- A new optional setting R² threshold can now be set. When this threshold is non-zero, the autofocus run has to fullfill a minimum required R² - [Coefficient of determination](https://en.wikipedia.org/wiki/Coefficient_of_determination) - above this threshold to be considered as successful.

### File name patterns and FITS keywords
- Added `CENTALT`, and `CENTAZ` keywords
- Added `AIRMASS` keyword, calculated from mount altitude using Gueymard 1993
- Added `$$CAMERA$$` file pattern
- Added `$$TELESCOPE$$` file pattern
- Added `$$ROTATEANGLE$$` file pattern
- Added `$$STARCOUNT$$` file pattern
- Added `$$TEMPERATURESETPOINT$$` file pattern

#### Included Camera SDK Versions can now be found inside the about page of N.I.N.A.

# Version 1.10 HF3

## Improvements
- The camera temperature setpoint is now displayed *only* when the cooler is active
- A secondary HIPS2 server will be attempted if the main HIPS2 image server is not available
- Canon cameras will now be kept awake to avoid camera auto-shutdown
- Meridian flip will now be considered on objects traversing through their lowest altitude too

### QHYCCD native driver improvements
- <span style="color:red">**IMPORTANT:**</span> NINA 1.10 HF3 (and later) require *at least* [QHY System Pack](https://www.qhyccd.com/download.html) version 21.02.20.19 to be installed
- Improvements to reliability
- Seamless switching between 11M and 47M modes for the QHY294 Pro
- QHY CCDs that have fast and slow readout modes can now select the mode
- QHY cameras that have sensor chamber air pressure and humidity sensors will display their readings on the Equipment > Camera screen
- QHY camera firmware and FPGA versions are now displayed on the Equipment > Camera screen
- The integrated filter wheels of A-Series cameras should now operate correctly
- Live View function has been disabled for QHY cameras. It was functionally inoperative and will be discontinued for all but Canon and Nikon in 1.11
- The version of the QHY USB driver will be checked and a warning will be presented if it is not the minimum recommended version

## Bug fixes
- Nikon: Cancelled or aborted exposures are now properly handled
- FLI: Stop background flush prior to reading out the sensor
- ASCOM camera: SensorType is no longer considered to be a mandadory property
- Fixed an issue when the location for ASTAP and ASPS where entered manually the validation was incorrect to assume a folder instead of a file
- When subsampling selection was still active after the app already subsampled an image it would further try to subsample. In some occasions this could cause a crash. The subsampling rectangle is now disabled after the first additional capture.

## Included Camera SDK Versions:
- **Altair Astro:** 48.18830.20210423
- **Atik:** 2020.08.3.642
- **Canon:** 13.13.0.6408
- **FLI:** 1.104.0.0
- **Nikon:** 1.3.2.3000
- **Omegon:** 39.15325.2019.810
- **QHY:** 21.02.19.19
- **ToupTek:** 48.18081.2020.1205
- **ZWO:** 1.16.3

# Version 1.10 HF2

## Improvements
- Rotator now displays mechanical position as well as sky position. Sky position will be displayed once the rotator is synced at least once.
- Updates to various vendor-supplied SDKs for bug fixes and new model support in the respective native drivers
- QHY: Legacy CCD and A-series cameras can now select Normal or Fast readout speeds when using the native driver

## Bugfixes
- The telescope will no longer go on a journey to the celestial pole before going to the park position
- Parking the telescope from the Equipment > Telescope screen will no longer cause the UI to freeze while the telescope is in the process of parking
- When rotator reports final position too early while still reporting IsMoving as true, N.I.N.A. will now still wait for the movement to complete
- SkyAtlas File Source will now consider binning for solving
- Fixed issue with filter wheel filter import in options when filters have the same name
- The MGEN2 guider can now auto select a guide star unattended, with a new calibration just downstream of that, which enables auto meridian flips
- The timeout values for the MGEN2 guider can now be overridden in the implementations of IMGENCommand
- The MGEN2 guider drift values are now correctly reported in fractional pixels
- ASCOM devices are now properly disposed after calling the setup dialog
- The MGEN guider now doesn't autoselect guide stars that lie too close to the sensor edge, based on a stand-off distance that the user can set in the UI
- All guiders now report when a successful connection happens
- Disconnecting Atik Cameras from other applications on startup or scan for new devices will not happen anymore
- Fixed an issue in Framing Assistant when solving a file, that the near solver was incorrectly used as the blind solver
- QHY: QHY294M/C Pro is properly handled by the QHY native driver

## Included Camera SDK Versions:
- **Altair Astro:** 48.18421.20210202
- **Atik:** 2020.08.3.642
- **Canon:** 13.13.0.6408
- **FLI:** 1.104.0.0
- **Nikon:** 1.3.2.3000
- **Omegon:** 39.15325.2019.810
- **QHY:** 21.02.19.19
- **ToupTek:** 48.18081.2020.1205
- **ZWO:** 1.16.3

# Version 1.10 HF1

## Features
- Ability to inspect pixel area and pixel values in detail by holding right click
- Relative Focusers can now be connected and will be simulated to behave like absolute focusers
- It is possible now to import Telescopius Observing Lists and Telescopius Mosaic Plans via CSV
- Added Ultra Mode and Dew Heater Controls for supported Altair Cameras.
- New translation for Greek (Ελληνικά) has been added
- Added a new sky survey <a href="http://alasky.u-strasbg.fr/hips-image-services/hips2fits" target="_blank">Hips 2 Fits</a> as an option in framing assistant. This survey seems to provide a much faster image compared to the other surveys.

## Improvements
- Canon CR3 image format is now supported (use FreeImage as your RAW converter)
- Adding support for the EOS R5 and EOS R6
- Added `$$CAMERA$$` file pattern
- Added `$$TELESCOPE$$` file pattern
- Added `$$ROTATEANGLE$$` file pattern
- Added `$$STARCOUNT$$` file pattern
- Telescope views now display the side of pier when available
- Replaced LZ4 library with a more optimized one for faster XISF compression
- Save target set in sequence screen is now enabled also when sequence is running
- Added info logs for start exposure, download location and star detection
- Rotators will now display the sky angle as the current position, when rotation via plate solving was invoked
- Added toggle to reverse direction for rotators

## Bugfixes
- When using a DSLR and Astrometry.net the uploaded file is now correctly sent as FITS instead of raw format
- Log files now show correct operating system version
- Cancelling a sequence that is paused, will no longer throw a semaphore error
- Set white balance and gamma to 50 for ASI native driver on connection
- FITS header parsing can now deal with keywords that don't have any comment at all
- Safeguard against NaN values for Hfr Std Deviation which could ruin an autofocus run
- Fix issue for QHY cameras to dither too early when camera was still exposing
- Images that are mirrored (e.g. on a scope like a hyperstar) now have a correct rotation inside the framing wizard
- Fixed a problem with Nikon Shutterspeeds between 1s and 30s not being parsed correctly in certain locale settings
- Fixed an issue when using the file camera together with dithering, where the dither signal was triggered directly after starting the exposure, as the file camera directly switched to download state. Instead the file camera will wait for the exposure time instead before switching to download state.

## Included Camera SDK Versions:
- **Altair Astro:** 48.18195.2020.1222
- **Atik:** 2020.08.3.642
- **Canon:** 13.12.31
- **FLI:** 1.104.0.0
- **Nikon:** 1.3.1.3001
- **Omegon:** 39.15325.2019.810
- **QHY:** 20.8.26.3
- **ToupTek:** 46.17309.2020.616
- **ZWO:** 1.16.3.0

# Version 1.10

## New hardware support

#### Flat Panel control and automation
- Control supported flat panel devices from within N.I.N.A.
- Flat Wizard can specify a specific panel brightness to attain optimal flat frame exposures
- Flat panels or covers that open and close will automatically do so at the beginning and end of a sequence
- The following flat panels are supported, with no additional software required:
    - All-Pro Spike-a Flat Fielder
    - Alnitak Flip-Flat, Flat-Man, and Remote Dust Cover
    - Artesky USB Flat Box
    - Pegasus Astro Flatmaster

#### Omegon veTEC and veLOX series cameras
- Native support for the line of veTEC and veLOX cameras from Omegon

#### Native support for QHY integrated and CFW filter wheels
- The integrated filter wheels that are in A-series cameras (QHY695A, QHY16200A, etc.) or are a CFW1/2/3 that is connected to a monochrome camera using the 4-pin cable, are now natively supported
- This allows the native QHY camera driver to be used with these cameras and filter wheel configurations.

#### Lacerta MGEN-2 Autoguider integration
- Full control of the MGEN-2 by mirroring the controller display into N.I.N.A.'s user interface
- Automatic power-on when connecting
- Starts guiding on sequence start
- Performs calibration and star auto-selection when required
- Displays star drift during guiding in a graph (currently only measured in pixels)
- Dithering during sequencing
- MGEN-3 support will appear in a future release

#### Native support for Pegasus Astro Ultimate Powerbox V2
- Connect to and control the Ultimate Powerbox V2 from within the application
- Monitor input voltage and power consumption
- Turn power and USB ports on and off via the Switch interface
- Set the output voltage of the variable power port
- Control the dew heater ports, including the Auto-Dew feature
- Support for using the Powerbox's sensors as a weather device
- Support for using the stepper motor driver as a focuser

#### Expanded native camera support
- **Altair Astro:** Added support for Altair Astro Hypercam 269 PRO TEC and other new cameras
- **Atik:** Updated vendor SDK for new camera support and bug fixes
- **Canon:** Added support for EOS M6 Mark II, EOS 90D, and EOS M200. CR3 RAW file is not supported at this time but is expected to be in a future release
- **Nikon:** Added support for the Z-series, D780, and D6
- **QHYCCD:** Added support for QHY268C, QHY600M/C, QHY367C-PRO, QHY4040 and others
- **ZWO:** Added support for the ASI533MC-Pro, ASI2600MC-Pro, and ASI6200MC/M-Pro
- Plus many bug fixes and feature enhancements in the respective camera vendor SDKs. Refer to the bottom of this release's section for the version numbers of the included vendor SDKs.


## Localization
- N.I.N.A. is now available on <a href="https://crowdin.com/" target="_blank">Crowdin</a>! This powerful online translation management tool allows users to easily contribute to the translation of N.I.N.A.'s user interface to any language. To help with localization and translation efforts, details may be found at <a href="https://nina.crowdin.com/" target="_blank">nina.crowdin.com</a>. Feel free to participate in the ongoing effort to provide multiple languages for N.I.N.A.! This is a great way to contribute to the project
- Thirteen new languages are included in this release:
    - Dansk (Danish)
    - Español (Spanish)
    - Français (French)
    - 日本語 (Japanese)
    - Nederlands (Dutch)
    - Polski (Polish)
    - Русский (Russian)
    - 简体中文, 中国 (Simplified Chinese, China)
    - 繁体中文, 台灣 (Traditional Chinese, Taiwan)
    - 繁体中文, 香港 (Traditional Chinese, Hong Kong)
    - Türk (Turkish) 
    - Galego (Galician) 
    - Portugese (Português)

## Application Improvements
### General
- Added the ability to record flat frame exposure times with the Flat Wizard, which may be later used in a sequence
- The Flat Wizard's limit of 50 exposures has been eliminated, but we're still not sure why you would want more than that
- The telescope will now stop tracking after being commanded to slew to zenith in the Framing Wizard
- Added the name of the active profile to the application title bar
- Warning notifications are now appear for 30 seconds instead of 3 seconds before automatically dismissing. Error notifications still display until they are dismissed by the user
- Logs older than 30 days are now automatically cleaned up when N.I.N.A. starts
- The timestamps in N.I.N.A.'s log file now have millisecond resolution and use the 24 hour time format
- In the **Equipment > Camera** window, the camera cooling is now activated only by using the Snowflake button, and what was the Cooler On/Off control now only reflects current state of the camera's cooler
- UI for temperature control has been split into cooling and warming sections. To cool the camera the user can specify a temperature and an optional duration, while for warming only an optional duration is required
- Clicking on warming or cooling without specifying a duration will show the cooling/warming progress based on target temperature and current camera temperature
- The current <a href="https://www.iers.org/IERS/EN/Science/EarthRotation/EarthRotation.html" target="_blank">Earth Rotation Parameter</a> data tables are now automatically downloaded from the IERS for use in various calculations. The check for new data tables happens at application start and if an internet connection exists
- An occasional miscalculation of the moon's current phase has been corrected
- The object altitude chart in Sequence and Sky Atlas now renders tick marks in half-hour increments instead of some random increment
- Added an empty entry in the Constellation filter in Sky Atlas to allow for its deselection
- Clean-ups of the status text in the status bar at the bottom of the main window
- Various spelling and grammar fixes throughout the application
- Added or clarified several missing tool tips
- Gain/Offset controls now display the current gain/offset the camera is set to when no gain/offset is specified

### Auto-focus system
- Auto-focus has been enhanced to support multiple curve fitting methodologies:
    - Parabolic fitting, weighted by standard deviation
    - Hyperbolic fitting, weighted by standard deviation
    - A combination of parabolic or hyperbolic fitting, with trend lines. The averages of fitting minimum and trend line intersection are then used
- A contrast detection auto-focus routine has been added. Instead of analyzing stars to determine the point of best focus, the routine will analyze the overall contrast of the image using various contrast detection methods. A Gaussian fit is then performed on the obtained focus points. On some systems, this process can employ shorter exposure times compared to Star HFR and produce results faster
- The single backlash compensation system has been improved and split into two selectable behaviors:
    - Absolute, where the focuser is moved in or out by the specified number of steps plus any additional steps for backlash. This is the same system that has existed before
    - Overshoot, a new method, where the focuser is moved in or out by the requested number of steps, plus any "overshoot" number of steps, and is then moved in the opposite direction by the same number of "overshoot" steps. This is suitable for optical systems such as SCTs, which might benefit from this method in order to eliminate mirror flop
- The "Crop Ratio" setting has been changed to "Inner Crop Ratio", and an additional "Outer Crop Ratio" setting has been added. This allows users to define a centered ROI, or a centered "square doughnut", which will be used by star detection. This allows for avoiding stars in the center and at the edges of the camera's FOV when a frame is analyzed during auto-focus
- HFR calculation is now computed using the mean background surrounding the star instead of the entire image's mean
- HFR calculation has been enhanced to provide more accurate results, especially for imaging systems that have central obstructions
- Auto-focus trend lines now use a weighted fit based on HFR standard deviation in each image rather than an unweighted fit. This provides much better slopes and final derived focus point
- A filter for use in auto-focusing can be set if "Use filter offsets" is set to true. When defined, the auto-focus routine will use the specified filter instead of the current imaging filter. Initial baseline HFR and final HFR (used to determine whether the auto-focus run was successful) will still use the intended imaging filter.
- Added the ability to keep guiding active during auto-focus operations
- The auto-focus routine has been changed so that it doesn't attempt to measure the focus twice for the same point on the curve
- If it is turned on, a focuser driver's internal temperature compensation feature is now turned off before an auto-focus operation starts and is turned back on when the operation completes
- Meridian Flip now has an option to trigger an auto-focus operation after a meridian flip while a sequence is running
- Backlash compensation will no longer be applied more than once if focuser movement is canceled during focuser settle time
- An auto-focus operation will not be performed if it might interfere with a pending meridian flip
- All auto-focus operations and their data points are now logged to their own <a href="https://www.json.org/" target="_blank">JSON</a>-formatted file located in `%LOCALAPPDATA%\NINA\AutoFocus`. This allows one to retrace what was measured at a later time

### Sequencer
- Parameters in rows that are added to a sequence will now default to the parameters specified in the row above it
- A user-specified command (batch script, Windows/DOS executable, etc.) may be ran at the conclusion of a sequence. This command may be specified under **Options > Imaging > Sequence**
- Sequence Gain and Offset settings can now be entered without having any camera connected. The values will be validated on sequence start in case an incompatible value was specified
- The pre-sequence checklist has been enhanced to check whether the telescope is parked. If it is parked, and the user affirms the action, N.I.N.A. will unpark the telescope before beginning the sequence
- Added a pre-sequence check to ensure that enough disk space is available to store the whole sequence's amount of images. The calculation does not take any file compression into account as that is unpredictable.
- End of Sequence operations are now done in parallel
- The "Auto-focus after % HFR Change" option has been enhanced to not be triggered by a single bad frame which might have been caused by a temporary condition such as wind or a passing vibration
- "Auto-focus after % HFR Change" now resets its reference index on sequence start, a target change, or any auto-focus operation that is caused by other criteria
- "Auto-focus After Number of Exposures" now correctly triggers after the specified number of exposures
- When the camera fails to indicate that an image is available for download after exposure time + 15 seconds, the exposure will be canceled and skipped
- When using filter offsets, NINA will no longer pause a sequence for any specified focuser settle time if there was no change in the focuser's position after switching filters
- Camera Gain and Offset are now saved as integers and are no longer saved with decimal places
- Added estimated target start and end times to sequence view. This is useful for multi-target sequence sets
- Guiding was improperly stopped when performing an auto-focus operation at start of sequence, even if the Disable Guiding option was false
- Sequence file names are now saved with valid file name characters. Invalid characters are replaced with a hyphen (-)
- Various sequence window presentation and layout improvements
- HFR calculation is enforced for light frames when autofocus after HFR change is enabled to ensure that the trigger will always work

### Plate Solving
- Additional information has been added to the plate solve pop-up window that appears during sequencing
- A camera gain and binning setting may be configured for use during automated plate solves
- Removed the "Repeat until" option from the **Imaging > Plate Solve** tool window. This action will be always occur when "Reslew to Target" is enabled
- Plate solvers will now receive an unstretched FITS image instead of a JPEG. This can allow for a quicker and more reliable solve result
- Any `WARNING` FITS keyword that a plate solver inserts into a solved image will result in a warning notice in N.I.N.A. that contains the keyword's text
- A custom Astrometry.net API URL may be configured
- Improved status feedback and logging for Astrometry.net plate solve jobs
- Plate Solving window's Sync and Reslew settings will be saved
- Reworked the plate solving code completely for cleaner operations

### Guiding
- Improved the resiliency of StartGuider when PHD2 is unable to detect a guide star when guiding is started due to temporary clouds or other factors. An auto-retry mechanism has been added to command PHD2 to attempt to select guide star after a configurable timeout
- The guiding graph will now pause during dithering to avoid displaying guide pulses that are not actual corrections
- The guiding graph now displays a gray triangle icon along the X axis at the point where a dither action occurred
- The drawing of lines in the guiding graph will now pause when guiding itself is paused
- The scaling of lines in the guiding graph will now be consistent even if there are missing data points
- Guiding graph settings are now stored in the profile and are loaded when application is started

### Framing Wizard
- NASA Sky Survey images now are automatically adjusted for brightness and contrast, depending on each image's characteristics
- When importing FITS or XISF files, any metadata they contain concerning the RA and declination of the image will be used to hint the plate solver. This can drastically speed up the importing of images into the Framing Wizard by avoiding blind solves
- If a FITS or XISF file contains WCS (World Coordinate System) information due to it already being plate solved externally, Framing Wizard will now retrieve and use that information instead of running its own plate solve on the image, resulting in instant positioning and rendering of the image
- The font size of the mosaic panel ID numbers is now scaled based on the imaging rectangle size and zoom level
- Coordinate input fields in the Framing Assistant and Sequence windows now allow for `-0` to be entered for declination
- When loading an image file into the Framing Assistant, the rectangle dimensions are now correctly calculated

### Device-related improvements
- The following camera settings are now saved to the active profile: USB Limit, Offset, Gain, Binning, Readout Mode, Temperature, Cooling and Warming Duration
- Images from OSC cameras now use their advertised bayer pattern instead of RGGB being assumed all the time
- N.I.N.A.'s camera simulator will now assume the pixel size, bayer matrix, and sensor temperature attributes of any FITS or XISF file that is loaded into it
- The equatorial system reported by the mount driver is now used, and the Epoch setting under **Options > General > Astrometry** has been removed. N.I.N.A. will default to J2000 when a mount's driver reports an epoch of "other"
- Wait and check to see if the mount flipped automatically at the meridian when "Use Telescope Side of Pier" is on
- A short timeout after mount sync operations has been added as some mounts may not immediately report updated coordinates after a Sync command is sent to them
- N.I.N.A.'s File Camera can now watch folders for Sony's `.ARW` and Olympus' `.ORF` RAW file formats
- Reintroduced a "No Guider" option under the **Equipment > Guiding** device list so that unguided setups do not suffer through unwanted PHD2 executions or unnecessary errors when the "Connect All Devices" button is pressed
- When a sequence is in the centering stage, rotators will now rotate to nearest orientation even if it results in an image that is upside down. An image's vertical orientation is not relevant for framing due to the corrective action of star alignment in post-processing
- Focuser control buttons are now disabled while the focuser is reporting that it is moving
- A new option is available to prevent sending any Sync command to the mount. The centering logic will then use offset a coordinate calculation instead. This new logic will also be called when a Sync command fails
- Added button to the **Equipment > Telescope** window to set the current position of the mount to be its park position. This button is available only if the connected mount's ASCOM driver supports setting a park position
- Canon camera support has been improved and many common errors have been fixed
- Atik cameras that have mechanical shutters will now close those shutters when taking BIAS, DARK, or DARKFLAT images
- FLI cameras no longer sit idle for the length of the exposure time prior to actually initiating the exposure
- QHY native camera driver now retrieves only the non-overscan area (the "effective area") of the sensor by default. A Camera option now exists to include the overscan area, if desired
- QHY native camera driver now supports selecting readout modes if the camera offers any
- QHY cameras that have mechanical shutters will now close those shutters when taking BIAS, DARK, or DARKFLAT images
- QHY native camera driver now optimizes image file size based on the actual image dimensions, resulting in slightly smaller image file sizes
- Binned exposures now finish on QHY cameras that have overscan areas
- ZWO cameras now properly handle odd bin dimensions (e.g. 3x3)
- Added a bit scaling option under **Options > Equipment > Camera > Advanced Settings**, intended for users of Altair Astro, Omegon, and ToupTek cameras, to bit-shift the raw sub-16 bit data camera data that N.I.N.A. receives to 16 bits in order to improve file compatibility with other capture software
- FreeImage library upgraded to 3.18 for improved DSLR raw file display
- ZWO cameras now write correct EGAIN values when switching Gain
- ZWO cameras reset the FLIP STATUS to NONE in case it was altered by another software
- Time to meridian flip is displayed in telescope windows when enabled

### Imaging window changes
- Added more southern hemisphere stars to the Manual Focus Stars tool
- Zooming to high magnification inside the Image viewer will no longer show smeared pixels. Sharp pixel edges will be shown instead
- The mid-tone stretch algorithm no longer inverts blown-out pixels
- The HFR History graph now displays indicators that denote when an auto-focus operation has occurred. Hovering over an indicator with the mouse pointer will reveal details for that operation, including the previous focuser position, the newly calculated focuser position, and the focuser's temperature at the time
- Optimal Exposure Calculator (OEC) has been moved out of the statistics window and into its own tool window for improved user experience, with improved calculations
- OEC can now load <a href="https://www.sharpcap.co.uk/sharpcap/features/sensor-analysis/" target="_blank">SharpCap Sensor Analysis</a> files to populate fields for the camera's full well and read noise parameters with measured values
- The camera snapshot control window will now save its parameters across application sessions
- Added a layout reset button for the Imaging window, located at **Options > Imaging > Layout**
- Images from OSCs are no longer debayered for presentation twice

### Planetarium software integration
- All Planetarium options have been moved to their own section in the **Options > Equipment** tab and the **Options > Planetarium** tab has been removed
- When using Stellarium, N.I.N.A. now takes current view's center coordinates when no target is selected
- Added an option to retrieve the coordinates of the center of the sky chart instead of any selected object when using TheSkyX
- Increased reliability when parsing coordinates that are retrieved from Cartes du Ciel and Stellarium, especially in cases where the user's locale uses characters other than the decimal point as the decimal separator
- Reworked the Planetarium interfacing to be more robust

### FITS and XISF support
- XISF files may be created with optional compression or shuffled compression of the image data using the LZ4, LZ4-HC, or ZLib (deflate) lossless compression algorithms. Decompression of compressed XISF files is also supported
- XISF files may be created with optional embedded checksums of the image data using SHA1, SHA-256, or SHA-512 hashing algorithms. Checksums are verified when opening a XISF file that includes one
- Opening XISF and FITS files that have non-16 bit data is now supported. This benefits the Framing Wizard and N.I.N.A. Simulator Camera
- XISF image properties for aperture and focal length now correctly state values in meters rather than millimeters

### File name patterns and FITS keywords
- `$$DATEMINUS12$$`: Shifts the current date and time to 12 hours into the past. This allows for all images from a day-crossing session to be saved into the same date folder
- `$$READOUTMODE$$`: Allows the camera's readout mode to be used in file or folder names
- `$$SQM$$`: Allows the current reading from an attached sky quality meter to be used in file or folder names
- `$$USBLIMIT$$`: Allows the USB Limit setting for the camera, if available, to be use in file or folder names
- The `$$FRAMENR$$` pattern now creates a 4 digit number, padding with zeros. Example: `0004`
- The "Image File Pattern" setting will now correctly save its state to the active profile when file patterns are dragged from the list into the "Image File Pattern" field
- A new example preview of the currently-assembled file pattern appears below the "Image File Pattern" field. Any folder separators that are specified are denoted by a `›` character
- File pattern values are now scrubbed for leading and trailing white spaces and other illegal characters to prevent the creation of an invalid file or folder name
- Added the `READOUTM` keyword to FITS and XISF files to record the name of the readout mode used to create the image
- Added the `BAYERPAT`, `XBAYEROFF`, and `YBAYEROFF` keywords to FITS and XISF files when using color cameras. A menu under **Options > Equipment > Camera** allows the user to override the driver-specified Bayer pattern with an alternative pattern. This allows for automatic debayering in some processing applications, including the use of the "Auto" setting in PixInsight's Debayer process.
- Added the `USBLIMIT` keyword to FITS and XISF files to record the USB Limit setting of the camera
- The FITS `XPIXSZ` and `YPIXSZ` keywords (and related XISF properties) now correctly account for the binning factor

## Special Thanks
The N.I.N.A. team would like to sincerely thank:

- The staff at <a href="https://teleskop-austria.com/" target="_blank">Teleskop Austria</a> for providing an MGEN-2 unit as well as a detailed communication protocol document for implementing and fully testing MGEN-2 support in N.I.N.A.
- <a href="https://www.qhyccd.com/" target="_blank">QHYCCD</a> for providing a QHY183M and CFW3 filter wheel to test and verify NINA's native QHY camera and filter wheel drivers

These items helped a lot during development and testing.  
Thank you for your support!

## Included Camera SDK Versions:
- **Altair Astro:** 46.16909.2020.404
- **Atik:** 2020.6.18.0
- **Canon:** 13.12.10
- **FLI:** 1.104.0.0
- **Nikon:** 1.3.1.3001
- **Omegon:** 39.15325.2019.810
- **QHY:** 20.6.26.0
- **ToupTek:** 46.17309.2020.616
- **ZWO:** 1.15.6.17

# Version 1.9

## Features

### Camera Control
- Native support for Finger Lakes Instrumentation ProLine/MicroLine cameras and color filter wheels

### Switch Hubs
- A new equipment type "Switch" has been added which can connect to an AscomSwitch or to a PrimaLuceLab EAGLE PC

### Dither without guiding
- A new Direct Guider has been added, which doesn't require a guide camera. It can only perform random dithers. It connects directly to the telescope, and will if required perform a dither via a Pulse Guide of a user-provided duration in a random direction.
- Enhanced direct guider will accept decimal durations (e.g. 0.5s), and perform random angle selection in a way that minimizes target deviation from center, even after many dither operations.

### Plate Solving
- Added interface for ASTAP, the Astrometric STAcking Program, astrometric solver and FITS viewer as a plate solver
- Mid-sequence plate solve operations (when slewing to target, or after Meridian Flip) have been enhanced to have the following behavior:
  - If plate solve fails, it automatically falls back to blind failover
  - If blind failover also fails, plate solve can be set to await a certain time period (by default 10 minutes) before trying again, up to a certain number of attempts (user-defined)
  - If all attempts fail, or Meridian is getting close, plate solve will be considered failed, but sequence will continue as usual
- Added options to adjust downsample factor and maximum number of considered stars for ASTAP and local astrometry.net solvers

### GPS Assisted Location
- Added a NMEA GPS interface to retrieve the current location

### Interfacing with planetarium programs
- NINA can interface with Cartes du Ciel, HNSKY, Stellarium, and TheSkyX through their repsective TCP services to import the selected object for use in the Sequence Editor and Framing Assistant, as well as setting the observing location to match that which is set in those programs

### Manual Camera
- Inside the camera selection there is a new entry for "Manual Camera".
- This simulated camera will enable the use of cameras which lack an SDK, while still using the whole N.I.N.A. workflow.
- It will watch a specified folder for newly created files. These files will be stored inside an internal queue.
- Each time the application wants to download an exposure from the camera, the first item of this file queue is resolved and loaded into N.I.N.A.
- Additionally a manual Bulb Mode trigger can be activated, so it will use the selected Bulb Mode in Settings-&gt;Equipment-&gt;Camera on Exposure Start.
- When this trigger is deactivated the application will just skip the Start Exposure and wait for another file to roll into the specified folder

### Weather data sources
- The existing OpenWeatherMap implementation has been replaced with a full weather data interface.
- The new interface allows devices with ASCOM ObservingConditions class drivers to supply N.I.N.A. with weather data and other conditions.
- Native OpenWeatherMap functionality is maintained, and any configured OWM API key is retained and utilized by the new native OWM client.
- Weather data sources are now configured under the Equipment section.
- Any available weather data types (air temperature, pressure, wind speed, etc.) are inserted into images as FITS keywords and/or XISF image properties.

### Focusing
- Quick focuser movement buttons have been added (fine/coarse move IN/OUT) to the focuser views
- A new focuser settle time parameter has been added, in case the focuser shifts the image when moving (SCT, lens belt focusing, etc.). This should help with auto-focus in particular.
- Backlash can be compensated using two methods. Absolut and Overshoot. Absolut will add absolute backlash values to the movement while overshoot will overshoot the position by a larger amount and move backwards again.
- Focuser backlash (in and out) can now be specified. The backlash will be applied to focuser movements depending on the method specified.
- Sequence Auto-Focus can now be triggered if measured HFR of any frame is X% worse than the first frame taken after the previous auto-focus routine.
- More resilient autofocus:
  - Ability to automatically reattempt autofocus from scratch several times in case it failed
  - Automatically go back to original focus position if obtained HFR is significantly worse than original
  - Ability to take multiple autofocus exposures per focus point and average their HFR. This leads to smoother autofocus curves.
  - A crop ratio has been added to the autofocus, letting users autofocus only on the center of the frame
  - Autofocus can now be set to use only the top brightest stars, so the same stars are used throughout the autofocus routine. This will work best on sparse star fields, and for stable, well-aligned equipment (e.g. so stars don't move between auto-focus exposures)
  - Ability to use binning for autofocus

### Sequencing
- End of Sequence Options are now available, which include:
  - Parking the telescope - This will stop the guiding, and invoke the mount Park method if available, otherwise the mount will slew to near the celestial pole (on the same side of Meridian it last was) and stop tracking. **Before using this in production, test out the feature at the telescope, with your finger on the power switch. This is to avoid any crash into the pier for mounts that do not have limits.**
  - Warming the camera - the camera will be slowly cooled, with the cooler eventually turned off
- A pre-sequence check is now triggered, and will notify end users of a variety of potential issues (camera not cooled yet, guider not connected, telescope not connected but slew enabled, etc.) at sequence start
- It is now possible to reset the progress of a sequence item, or of a whole sequence target. If an item that occurred prior to the active sequence item is reset, stopping and starting the sequence will get back to it, but pausing/playing will keep going from the current item.
- The sequence start button is unavailable if an imaging loop is in progress in the imaging tab
- If telescope is capable of reporting SideOfPier there will now be a new option to consider this for calculating the need for meridian flips
- It is now possible to set the Offset in addition to the Gain within each sequence item
- Added buttons to move sequence row up and down the list 
- File handling now changed so that:
  - the default folder for sequences is set under Options -> Imaging
  - a 'modified' status is maintained for each target
  - targets can be loaded from any xml file
  - targets can be saved back to the file it was loaded from
  - a 'Save as' option is added to save to a new file
  - a warning is issued if a target is closed without saving when it has been modified.  This also applies when the application is closed.
- Controls to change order of targets in a multi-target sequence
- Ability to save and load 'target sets' (a set of targets in a certain sequence)

### Flat Wizard
- Progress bars have been added for remaining filters and exposures
- A new Slew to Zenith button has been added for easier flats. This includes an option for east or west pier side, depending on which side of pier the mount should approach zenith from.
- A new option to pause between filters has been added. This is to allow the user the chance to set lightbox settings 

### Interface
- Imaging tab - Equipment specific views will only show the "Connected" flag when the device is not connected to save space
- Added a layout reset button to the imaging tab to restore the default dock layout.
- Equipment chooser dropdowns are now grouped by driver categories to easily distinguish between for example ASCOM drivers and other vendor drivers

### File Handling
- All FITS keywords now have descriptive comments with units of measurement noted if applicable
- FITS keyword `DATE-OBS` now has millisecond resolution, eg: `2019-03-24T04:04:55.045`
- Additional FITS keywords are now added to images if their associated data is available:
	- `DATE-LOC`: Date and time of exposure adjusted for local time
	- `FOCRATIO`: Focal ratio of the telescope, user-configurable under Options->Equipment->Telescope
	- `SET-TEMP`: The configured CCD cooling setpoint
	- `SITEELEV`: Elevation of the observing site, in meters
	- `SWCREATE`: Contains `N.I.N.A. <version> <architecture>`
	- `TELESCOP`: Telescope name if provided under Options->Equipment->Telescope. Falls back to ASCOM mount driver name

### OSC Camera Handling
- Debayering is now applied prior to plate-solving or auto-focus star detection
- An Unlinked Stretch option has been added. When enabled, color channels will be stretched separately, helping hide the sky background. This results in more visible celestial objects, and helps enhance both autofocus and platesolving reliablity, especially in light polluted areas. Processing time is however increased.
- A Debayered HFR option has been added. When enabled, the HFR computation will be made on the Debayered image rather than the Bayered array, providing better Auto-focus results

### Star Detection Sensitivity
- Star Detection has been enhanced to detect more stars more accurately, while avoiding picking up noise by checking that the star is a local maximum and has sufficient bright pixels
- In addition, a new Star Sensitivity Parameter is available in the Imaging options. It has three settings:
	- Normal: use standard NINA star detection
	- High: More sensitive method, with little to no performance impact. Typically picks up 1.5x - 2.5x more stars than Normal
	- Highest: Most sensitive method, with some performance impact. Typically picks up 1.5x - 2.5x more stars than High
- The higher the detection level, the more likely lumps of noise are liable to be picked up (despite rejection parameters to avoid that)
- A noise reduction parameter has been added for better star detection in fairly noisy images, which can be important if using High or Highest star sensitivity levels, although the Normal sensitivity level will also benefit from it. Several settings are available:
	- None: no noise reduction on full size image done before star detection
	- Median: a Median filter is applied to the full size image before star detection. This is good if the sensor has many hot pixels, but time consuming
	- Normal: a standard fast Gaussian filter is applied to the full size image before star detection. This is good for smoothing out the thermal and bias noise of the sensor
	- High: a strong fast Gaussian filter is applied to the full size image before star detection. This is for fairly noisy images, but can make star detection more difficult
	- Highest: a very strong fast Gaussian filter is applied to the full size image before star detection. This is for noisy images, although star detection may suffer


## Bugfixes
- Fixed when FramingAssistant was not opened before and a DSO was selected from the SkyAtlas as Framing Source an error could occur
- Fixed scrolling through Framing Assistant Offline Sky Map while cursor was inside Rectangle ignored zooming
- Fixed Alitude charts displaying wrong Twilight/Night predictions for some scenarios
- Manual focus target list was not updating in some scenarios. Now it will always update. The interval for updates is one minute.
- Fixed an issue in FramingAssistant when reloading the image and having multiple panels selected, that the orientation was not considered properly resulting in wrong coordinates
- Fixed an issue in the Telescope Equipment tab that could potentially slew to the wrong Declination if the declination angle was negative
- Fixed issues in the subsampling logic for ASCOM, ZWO, and QHY cameras - the origin coordinates are now properly set, and take binning into account
- Fixed a race condition that caused HFR to not be computed for frames right before autofocus in some instances
- Fixed an issue for Nikon SDK that looked into the wrong folder for the external md3 files.
- Fixed a bug where Platesolve Orientation was displayed as negative and also throwed of the rotation centering when using rotators.
- Fixed a bug in Autofocus routine that could wrongly declare an autofocus run a failure if the starting point couldn't detect any stars
- Fixed custom color schema not saving properly and resetting to default when reloading the application
- The backlash measurement routine has been fixed so that the focuser is properly recentered before the backlashOUT measurement procedure
- Some Sky Surveys did not work in some locales due to decimal pointer settings 
- Fixed race condition when using DCRaw when the previous temp image was not finished processed and the new image tried to replace the previous temp image

## Improvements
- When EOS Utility is running in the background, the x64 N.I.N.A. client will scan for this app and prevent a crash due to the EOS utility being open. Instead a notification will show up to close the EOS Utility.
- N.I.N.A. SQLite Database will be created on demand and migrated to new versions on application startup instead of just being overwritten by the installer.
- Setup Installer can be run in fewer clicks and is also capable of launching the application after successful installation.
- Image History Graph will only contain statistics from sequence items
- Further increased parallelism during sequencing. 
	- After capture during image download: Parallel dither and change filter (if required)
	- After download: Parallel image saving and processing to display the image
- Image HFR is now available as an image file name token (`$$HFR$$`)
- Focuser Temperature is now available as an image file name token (`$$FOCUSERTEMP$$`)
- Added a clear button on HFR History graph. The button will be displayed when hovering the control.
- A new button is added in the options to directly open the log destination folder
- Added option to adjust USBLimit for Altair and Touptek cameras. This can prevent potential black preview screen issues. More details on this topic described at altair: https://altaircameras.com/black_preview_screen/

## Special Thanks
The N.I.N.A. team would like to sincerely thank:
- The staff at [Cloud Break Optics](https://cloudbreakoptics.com/) and [Finger Lakes Instrumentation](http://flicamera.com/) for arranging a ProLine PL09000 and CFW1-5 to assist in integrating native FLI camera and filter wheel support.
- Filippo Bradaschia from [PrimaLuceLab](https://www.primalucelab.com/) for providing an EAGLE unit to implement direct interfacing with the EAGLE Manager

## Included Camera SDK Versions:
- Altair: 46.17427.2020.704
- Atik: 8.7.3.5
- Canon: 3.8.20.0
- FLI: 1.104.0.0
- Nikon: 1.3.0.3001
- QHY: 0.5.1.0
- ToupTek: 30.13342.2018.1121
- ZWO: 1.14.7.15

# Version 1.8 Hotfix 1

- Prevent an unnecessary profile saving on application start
- Profiles where always saved after a sequence exposure, which slows down the image saving process. During sequence capture the profile saving is disabled now.

# Version 1.8

## Features

### Sequence
- Enable/Disable sequence entries

### Plate Solving
- Added interface for All Sky Platesolver

### Camera Control
- Altair native driver support
- ToupTek native driver support
- QHYCCD native driver support
- Added support for anti-dew heaters in ZWO cameras
- On ASCOM drivers support for setting readout modes

### Framing Assistant
- Add SkyAtlas image source which allows for framing based on offline SkyAtlas data

### Flat Wizard
- Supports you taking flats

### Imaging
- Added a list of manual focus targets (bright stars) that are currently visible in the sky

### Framing Assistant Offline Sky Chart
- Based on Sky Atlas data a basic sky chart showing objects can be displayed
- Instead of dragging the rectangular through the initial image like in the other framing sources
  the background itself will be moved like in an orrery  

### Guiding
 - Added Synchronized PHD2 Guider (experimental)
 - Synchronized PHD2 Guider will synchronize your Dither requests between multiple instances of N.I.N.A.
	- Known limitations: Dithering will happen every possible synchronized frame and is not changeable

### Auto-Update Channels
 - Previously the auto update was always just looking for released versions. Now multiple sources (Release, Beta and Nightly) can be selected and the auto updater updates to the respective version accordingls.
 - Additionally the changelog for the new version will now be displayed prior to updating, too. 


## Bugfixes

- Corrected Max Binning level for ASI Cameras
- Focuser move command fixed where lots of move commands where sent by accident
- Rotation value now considered for sequence import|export
- Canon: Fixed bulb mode for exposure times <30s
- Canon: All shutter speeds now correctly added when step set custom function is set to 1/3
- Meridian Flip window does not get stuck anymore when clicking on cancel
- Log Level will now be set on application start based on profile settings.
- ASI Cameras will not shut down their cooling and progress on opening multiple instances of NINA
- In some cases the application stayed open in the background after closing. This should not happen anymore
- `$$DATETime$$` and `$$TIME$$` will now use timestamp on exposure start, not on exposure end
- Fixed Meridian Offset default values for plate solved polar alignment 
- On sequence target guiding will be paused, prior to slewing to a new target
- Autofocus will not trigger a pause/resume command anymore, as this was not reliable in some cases. Instead PHD2 is stopped and started, similar to what is done during meridian flip.
- Fixed Platesolve result reporting wrong Dec Error.
- Platesolve recenter now considers proper angular distance calculation instead of relying just on Right Ascension.

## Improvements

- Clear button for PHD2 Graph
- Hide camera cooler controls when not available for current camera model
- Zero Floating point numbers now displayed as "0.00" instead of ".00"
- Show better exception message when an ASCOM Interop Exception occurs
- Canon: Errors are now shown to users in a readable format if any occur
- Removed hard requirement of ASCOM platform. Application can now function without it
- Improved UI Style. 
    - New logo
    - Better version display
    - Tweaked some color themes for more consistent colors
    - Better spacing between elements to reduce wasted space
    - Two new background colors to better pronounce some ui elements
    - Reworked Imaging tab to have a common style.
    - Imaging tab tools pane (to hide/show panels) moved to the top and split into two separate categories
- Profiles don't get overriden when using multiple instances of N.I.N.A. with each one having a separate profile active
- Autostretch replaced by a better midpoint transformation function
- Autostretch now has black point clipping options
- Vastly improved image statistics calculation time.
- Estimated Finish Time will automatically update in the sequencing view
- Added copy button for existing color schemas to copy over to custom and alternative custom schemas    
- Framing tab: 
	- Moved coordinates out of framing boxes to not obscure target
	- Added a new button to be able to add the framing target to a sequence instead of replacing
	- Control to adjust opacity of framing box
- Improved Framing Assistant and Sequence Target Textboxes by giving up to 50 target hints based on input to select from
- Framing Assistant now can annotate DSO
- Sensor offset is now available as an image file name token (`$$OFFSET$$`)
- Attempt to start PHD2 and connect all equipment when connecting to guider and PHD2 is not running
- Automatically import filter wheel filters to the profile on connection when profile filter list is still empty
- Load a default imaging tab layout in case the layout file is corrupted or not compatible anymore
- Removed Altitude Side combobox from plate solved polar alignment. It will be automatically determined based on alt/az coordinates.
- Changed log file format. Each application start will write to a separate log file for better distinction
- Improved XISF save speed and resulting file size by not embedding the image as base64 string, but instead as attached raw byte data
- Additional FITS keywords are now added to images if their associated data is available:
	- `OBJECT`: The name of the target, as supplied in the active Sequence
	- `DEC` and `RA`: The DEC and RA of the telescope at the time of the exposure
	- `INSTRUME`: The name of the connected camera
	- `OFFSET`: The sensor offset, if applicable
	- `FWHEEL`: Name of the connected filter wheel
	- `FOCNAME`: Name of the connected focuser
	- `FOCPOS` and `FOCUSPOS`: Position of the focuser, in steps
	- `FOCUSSZ`: Size of a focuser step, in microns
	- `FOCTEMP` and `FOCUSTEM`: Temperature reported by the focuser
	- `ROTNAME` Name of the connected rotator
	- `ROTATOR` and `ROTATANG`: Angle of the rotator
	- `ROTSTPSZ` Minimum rotator step size, in degrees
	- Applicable XISF Image Property analogs of the above, as defined by XISF 1.0 Section 11.5.3

## Special Thanks
The N.I.N.A. team would like to thank 

- Nick Smith from [Altair Astro](https://www.altairastro.com/) for providing a GPCAM2 290C to integrate Altair SDK
- Elias Erdnuess from [Astroshop.eu](https://www.astroshop.eu) for providing multiple Toupcam Cameras to integrate ToupTek SDK
- The staff at QHYCCD dealer [Cloud Break Optics](https://cloudbreakoptics.com/) for lending a QHY183C to integrate QHYCCD SDK

These items helped a lot during development and testing.  
Thank you for your support!

___

# Version 1.7

## Features

### Framing Assistant
- Mosaic planning for framing assistant
- Added multiple new SkySurveys to choose from for framing
- DSLR RAW files can now be loaded into framing assistant

### Sequences
- Sequence multiple target planning. Import/Export also available 
- Consider Rotation for framing assistant when set for a sequence during platesolves

### Filter Wheel
- Added a manual FilterWheel for users without a motorized wheel or with a filter drawer. There will pop up a window and prompt the user when a filter change is requested

### Rotators
- Support for ASCOM Rotators added
- A manual rotator option is added, for users without an automatic rotator. A pop up will show with the current angle and target angle for a user to manually rotate to

### Imaging
- DSLR Users: Images will now always be saved as RAW to prevent any loss of data due to conversions
- Aberration inspector added to imaging tab. It will show a 3x3 Panel containing the current image
- Battery display for DSLRs

### Settings
- Added ImageParameter `$$RMSARCSEC$$` and `$$FOCUSPOSITION$$`
- Latitude and Longitude can now be synced from application to telescope and vica versa (when supported).
- Serial Relay (via USB) interaction for Nikon Bulb

## Bugfixes

- Fixed Canon issue that a second exposure is accidentally triggered, causing the camera to be stuck
- Fixed memory leak when using Free Image RAW Converter

## Improvements

- Guider is also connected when pushing "Connect All"
- Changed FilterWheel UI for switching filters
- Some improvements to memory consumption
- Automatically triggered Autofocus and Platesolving now spawns inside a separate window
- Guiding Dither thresholds now configurable
- Flipped + and - for Stepper Controls
- Minor UI Layout improvements
- Made SkySurvey Cache directory configurable
- Local astrometry.net client will now downscale image for faster image solving
- ImagePatterns inside options can now be dragged from the list to the textbox
- Major code refactorings for better maintainability
- Lots and lots of minor bugfixes and improvements
- Launch NINA with a specific ProfileId via cmdline
- Integrated with Windows JumpList feature to launch instance with specific profile loaded
- Enhanced Altitude Check to permit imaging of targets that are below the configured altitude when they are rising in the sky and display a visual cue to distinguish rising versus setting targets
