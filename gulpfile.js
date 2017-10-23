var gulp = require("gulp");
var msbuild = require("gulp-msbuild");
var debug = require("gulp-debug");
var foreach = require("gulp-foreach");
var rename = require("gulp-rename");
var watch = require("gulp-watch");
var merge = require("merge-stream");
var newer = require("gulp-newer");
var util = require("gulp-util");
var runSequence = require("run-sequence");
var path = require("path");
var config = require("./gulp-config.js")();
var nugetRestore = require('gulp-nuget-restore');
var fs = require('fs');
var unicorn = require("./scripts/unicorn.js");
var habitat = require("./scripts/habitat.js");
var exec = require("child_process").exec;

module.exports.config = config;

gulp.task("default", function (callback) {
  config.runCleanBuilds = true;
  return runSequence(
    "01-Copy-Sitecore-Lib",
    "02-Nuget-Restore",
    "03-Publish-Website",
    "04-Apply-Xml-Transform",
    "05-Publish-Engine",
    "06-Sync-Unicorn",
    "07-Deploy-Transforms",
	callback);
});

gulp.task("deploy", function (callback) {
  config.runCleanBuilds = true;
  return runSequence(
    "01-Copy-Sitecore-Lib",
    "02-Nuget-Restore",
    "03-Publish-Website",
    "04-Apply-Xml-Transform",
    "05-Publish-Engine",
    "07-Deploy-Transforms",
	callback);
});

/*****************************
  Initial setup
*****************************/
gulp.task("01-Copy-Sitecore-Lib", function () {
  console.log("Copying Sitecore Libraries and License file");

  fs.statSync(config.sitecoreLibraries);

  var files = config.sitecoreLibraries + "/**/*";

  var libs = gulp.src(files).pipe(gulp.dest("./lib/Sitecore"));
  var license = gulp.src(config.licensePath).pipe(gulp.dest("./lib"));

  return merge(libs, license);
});

gulp.task("02-Nuget-Restore", function (callback) {
  var solution = "./" + config.solutionName + ".sln";
  return gulp.src(solution).pipe(nugetRestore());
});

gulp.task("03-Publish-Website", function (callback) {
  return runSequence(
    "Build-Solution",
    "Publish-Storefront-Projects",
    "Publish-Foundation-Projects",
    "Publish-Feature-Projects",
    "Publish-Css",
    "Publish-Project-Projects", callback);
});

gulp.task("04-Apply-Xml-Transform", function () {
  var layerPathFilters = ["./src/Foundation/**/*.transform", "./src/Feature/**/*.transform", "./src/Project/**/*.transform", "!./src/**/obj/**/*.transform", "!./src/**/bin/**/*.transform"];
  return gulp.src(layerPathFilters)
    .pipe(foreach(function (stream, file) {
      var fileToTransform = file.path.replace(/.+Website\\(.+)\.transform/, "$1");
      fileToTransform = fileToTransform.replace(/.+legacy\\(.+)\.transform/, "$1");
      util.log("Applying configuration transform: " + file.path);
      return gulp.src("./scripts/applytransform.targets")
        .pipe(msbuild({
          targets: ["ApplyTransform"],
          configuration: config.buildConfiguration,
          logCommand: false,
          verbosity: "minimal",
          stdout: true,
          errorOnFail: true,
          maxcpucount: 0,
          toolsVersion: 14.0,
          properties: {
            Platform: config.buildPlatform,
            WebConfigToTransform: config.websiteRoot,
            TransformFile: file.path,
            FileToTransform: fileToTransform
          }
        }));
    }));
});

gulp.task("05-Publish-Engine", function (callback) {
    return exec('dotnet publish .\\src\\Project\\Retail\\Engine -o ' + config.commerceEngineRoot, function (err, stdout, stderr) {
        console.log(stdout);
        console.log(stderr);
        callback(err);
    });
});

gulp.task("06-Sync-Unicorn", function (callback) {
  var options = {};
  options.siteHostName = habitat.getSiteUrl();
  options.authenticationConfigFile = config.websiteRoot + "/App_config/Include/Unicorn/Unicorn.UI.config";
  options.maxBuffer = Infinity;
  unicorn(function() { return callback() }, options);
});


gulp.task("07-Deploy-Transforms", function () {
  return gulp.src(["./src/**/Website/**/*.transform", "./src/**/legacy/**/*.transform"]).pipe(gulp.dest(config.websiteRoot + "/temp/transforms"));
});

/*****************************
  Copy assemblies to all local projects
*****************************/
gulp.task("Copy-Local-Assemblies", function () {
  console.log("Copying site assemblies to all local projects");
  var files = config.sitecoreLibraries + "/**/*";

  var root = "./src";
  var projects = [root + "/**/Website/bin", root + "/**/legacy/bin"];
  return gulp.src(projects, { base: root })
    .pipe(foreach(function (stream, file) {
      console.log("copying to " + file.path);
      gulp.src(files)
        .pipe(gulp.dest(file.path));
      return stream;
    }));
});

/*****************************
  Publish
*****************************/
var publishProjects = function (location, dest) {
  dest = dest || config.websiteRoot;
  var targets = ["Build"];

  console.log("publish to " + dest + " folder");
  return gulp.src([location + "/**/Website/**/*.csproj", location + "/**/legacy/*.csproj", location + "/**/*.csproj"])
    .pipe(foreach(function (stream, file) {
      return stream
        .pipe(debug({ title: "Building project:" }))
        .pipe(msbuild({
          targets: targets,
          configuration: config.buildConfiguration,
          logCommand: false,
          verbosity: "minimal",
          stdout: true,
          errorOnFail: true,
          maxcpucount: 0,
          toolsVersion: 14.0,
          properties: {
        Platform: config.publishPlatform,
            DeployOnBuild: "true",
            DeployDefaultTarget: "WebPublish",
            WebPublishMethod: "FileSystem",
            DeleteExistingFiles: "false",
            publishUrl: dest,
            _FindDependencies: "false"
          }
        }));
    }));
};

gulp.task("Build-Solution", function () {
	console.log("Building Solution");
  var targets = ["Build"];
  if (config.runCleanBuilds) {
    targets = ["Clean", "Build"];
  }
  var solution = "./" + config.solutionName + ".sln";
  return gulp.src(solution)
      .pipe(msbuild({
          targets: targets,
          configuration: config.buildConfiguration,
          logCommand: false,
          verbosity: "minimal",
          stdout: true,
          errorOnFail: true,
          maxcpucount: 0,
          toolsVersion: 14.0,
          properties: {
            Platform: config.buildPlatform
          }
        }));
});

gulp.task("Publish-Storefront-Projects", function () {
	console.log("Publishing Storefront Projects");
  return publishProjects("./src/Foundation/Commerce/storefront/{CommonSettings,CF/CSF}");
});

gulp.task("Publish-Foundation-Projects", function () {
  return publishProjects("./src/Foundation");
});

gulp.task("Publish-Feature-Projects", function () {
  return publishProjects("./src/Feature");
});

gulp.task("Publish-Project-Projects", function () {
  return publishProjects("./src/Project");
});

gulp.task("Publish-Assemblies", function () {
  var root = "./src";
  var binFiles = root + "/**/Website/**/bin/Sitecore.{Feature,Foundation,Project}.*.{dll,pdb}";
  var destination = config.websiteRoot + "/bin/";
  return gulp.src(binFiles, { base: root })
    .pipe(rename({ dirname: "" }))
    .pipe(newer(destination))
    .pipe(debug({ title: "Copying " }))
    .pipe(gulp.dest(destination));
});

gulp.task("Publish-All-Views", function () {
  var root = "./src";
  var roots = [root + "/**/Views", "!" + root + "/**/obj/**/Views"];
  var files = "/**/*.cshtml";
  var destination = config.websiteRoot + "\\Views";
  return gulp.src(roots, { base: root }).pipe(
    foreach(function (stream, file) {
      console.log("Publishing from " + file.path);
      gulp.src(file.path + files, { base: file.path })
        .pipe(newer(destination))
        .pipe(debug({ title: "Copying " }))
        .pipe(gulp.dest(destination));
      return stream;
    })
  );
});

gulp.task("Publish-All-Configs", function () {
  var root = "./src";
  var roots = [root + "/**/App_Config", "!" + root + "/**/obj/**/App_Config"];
  var files = "/**/*.config";
  var destination = config.websiteRoot + "\\App_Config";
  return gulp.src(roots, { base: root }).pipe(
    foreach(function (stream, file) {
      console.log("Publishing from " + file.path);
      gulp.src(file.path + files, { base: file.path })
        .pipe(newer(destination))
        .pipe(debug({ title: "Copying " }))
        .pipe(gulp.dest(destination));
      return stream;
    })
  );
});

gulp.task("Publish-Css", function () {
    var root = "./src";
    var roots = [root + "/**/styles", "!" + root + "/**/obj/**/styles"];
    var files = "/**/*.css";
    var destination = config.websiteRoot + "\\styles";
    return gulp.src(roots, { base: root }).pipe(
      foreach(function (stream, file) {
          console.log("Publishing from " + file.path);
          gulp.src(file.path + files, { base: file.path })
            .pipe(newer(destination))
            .pipe(debug({ title: "Copying " }))
            .pipe(gulp.dest(destination));
          return stream;
      })
    );
});

/*****************************
 Watchers
*****************************/
gulp.task("Auto-Publish-Css", function () {
  var root = "./src";
  var roots = [root + "/**/styles", "!" + root + "/**/obj/**/styles"];
  var files = "/**/*.css";
  var destination = config.websiteRoot + "\\styles";
  gulp.src(roots, { base: root }).pipe(
    foreach(function (stream, rootFolder) {
      gulp.watch(rootFolder.path + files, function (event) {
        if (event.type === "changed") {
          console.log("publish this file " + event.path);
          gulp.src(event.path, { base: rootFolder.path }).pipe(gulp.dest(destination));
        }
        console.log("published " + event.path);
      });
      return stream;
    })
  );
});

gulp.task("Auto-Publish-Views", function () {
  var root = "./src";
  var roots = [root + "/**/Views", "!" + root + "/**/obj/**/Views"];
  var files = "/**/*.cshtml";
  var destination = config.websiteRoot + "\\Views";
  gulp.src(roots, { base: root }).pipe(
    foreach(function (stream, rootFolder) {
      gulp.watch(rootFolder.path + files, function (event) {
        if (event.type === "changed") {
          console.log("publish this file " + event.path);
          gulp.src(event.path, { base: rootFolder.path }).pipe(gulp.dest(destination));
        }
        console.log("published " + event.path);
      });
      return stream;
    })
  );
});

gulp.task("Auto-Publish-Assemblies", function () {
  var root = "./src";
  var roots = [root + "/**/Website/**/bin", root + "/**/legacy/**/bin"];
  var files = "/**/Sitecore.{Feature,Foundation,Project,Habitat}.*.{dll,pdb}";;
  var destination = config.websiteRoot + "/bin/";
  gulp.src(roots, { base: root }).pipe(
    foreach(function (stream, rootFolder) {
      gulp.watch(rootFolder.path + files, function (event) {
        if (event.type === "changed") {
          console.log("publish this file " + event.path);
          gulp.src(event.path, { base: rootFolder.path }).pipe(gulp.dest(destination));
        }
        console.log("published " + event.path);
      });
      return stream;
    })
  );
});

/*****************************
 Commerce
*****************************/
gulp.task("CE-Install-Commerce-Server", function (callback) {
    var options = { maxBuffer: 4024 * 1024 };
    return exec("powershell -executionpolicy unrestricted -file .\\install-commerce-server.ps1", options, function (err, stdout, stderr) {
        if (err) {
            console.error("exec error: " + err);
            throw err;
        }
        console.log("stdout: " + stdout);
        console.log("stderr: " + stderr);
        callback();
    });
});

gulp.task("CE-Install-Commerce-Sites", function (callback) {
    var options = { maxBuffer: 4024 * 1024 };
    return exec("powershell -executionpolicy unrestricted -file .\\install-commerce-sites.ps1", options, function (err, stdout, stderr) {
        if (err) {
            console.error("exec error: " + err);
            throw err;
        }
        console.log("stdout: " + stdout);
        console.log("stderr: " + stderr);
        callback();
    });
});

gulp.task("CE-Uninstall-Commerce", function (callback) {
    var options = { maxBuffer: 1024 * 1024 };
    return exec("powershell -executionpolicy unrestricted -file .\\uninstall-commerce.ps1", options, function (err, stdout, stderr) {
        if (err) {
            console.error("exec error: " + err);
            throw err;
        }
        console.log("stdout: " + stdout);
        console.log("stderr: " + stderr);
        callback();
    });
});

gulp.task("CE~default", function (callback) {
    config.runCleanBuilds = true;
    return runSequence(
      "CE-01-Nuget-Restore",
      "CE-02-Publish-CommerceEngine-Projects",
      callback);
});

gulp.task("CE-01-Nuget-Restore", function (callback) {
    return runSequence("02-Nuget-Restore", callback);
});

gulp.task("CE-02-Publish-CommerceEngine-Projects", function (callback) {
    var cmd = "dotnet publish ./src/Project/Retail/Engine -o " + config.commerceEngineRoot
    var options = { maxBuffer: 1024 * 1024 };
    console.log("cmd: " + cmd);
    return exec(cmd, options, function (err, stdout, stderr) {
        if (err) {
            console.error("exec error: " + err);
            throw err;
        }
        console.log("stdout: " + stdout);
        console.log("stderr: " + stderr);
        callback();
    });
});

gulp.task("CE-Import-CSCatalog", function (callback) {
    var dataPath = config.commerceDatabasePath + "\\Catalog.xml";
    var command = "\"& {Import-Module CSPS; Import-CSCatalog -Name " + config.commerceServerSiteName + " -File " + dataPath + " -ImportSchemaChanges $true -Mode Full}\""
    var cmd = "powershell -executionpolicy unrestricted -command " + command
    var options = { maxBuffer: 1024 * 1024 };
    console.log("cmd: " + cmd);
    return exec(cmd, options, function (err, stdout, stderr) {
        if (err) {
            console.error("exec error: " + err);
            throw err;
        }
        console.log("stdout: " + stdout);
        console.log("stderr: " + stderr);
        callback();
    });
});

gulp.task("CE-Export-CSCatalog", function (callback) {
    var dataPath = config.commerceDatabasePath + "\\Catalog.xml";
    var command = "\"& {Import-Module CSPS; Export-CSCatalog -Name " + config.commerceServerSiteName + " -File " + dataPath + " -SchemaExportType All -Mode Full}\""
    var cmd = "powershell -executionpolicy unrestricted -command " + command
    var options = { maxBuffer: 1024 * 1024 };
    console.log("cmd: " + cmd);
    return exec(cmd, options, function (err, stdout, stderr) {
        if (err) {
            console.error("exec error: " + err );
            throw err;
        }
        console.log("stdout: " + stdout);
        console.log("stderr: " + stderr);
        callback();
    });
});

gulp.task("CE-Import-CSInventory", function (callback) {
    var dataPath = config.commerceDatabasePath + "\\Inventory.xml";
    var command = "& {Import-Module CSPS; Import-CSInventory -Name " + config.commerceServerSiteName + " -File " + dataPath + " -ImportSchemaChanges $true -Mode Full}"
    var options = { maxBuffer: 1024 * 1024 };
    return exec("powershell -executionpolicy unrestricted -command \"" + command + "\"", options, function (err, stdout, stderr) {
        if (err) {
            console.error("exec error: " + err);
            throw err;
        }
        console.log("stdout: " + stdout);
        console.log("stderr: " + stderr);
        callback();
    });
});

/*****************************
  Kill Tasks
*****************************/
gulp.task("Kill-w3wp-Tasks", function (callback) {
    var cmd = "@tskill w3wp /a /v"
    var options = { maxBuffer: 1024 * 1024 };
    console.log("cmd: " + cmd);
    return exec(cmd, options, function (err, stdout, stderr) {
        if ((err) && (!stderr.includes("Could not find process"))) {
            console.error("exec error: " + err);
            throw err;
        }
        console.log("stdout: " + stdout);
        console.log("stderr: " + stderr);
        callback();
    });
});

gulp.task("Kill-iisexpress-Tasks", function (callback) {
    var cmd = "@tskill iisexpress /a /v"
    var options = { maxBuffer: 1024 * 1024 };
    console.log("cmd: " + cmd);
    return exec(cmd, options, function (err, stdout, stderr) {
        if ((err) && (!stderr.includes("Could not find process"))) {
            console.error("exec error: " + err);
            throw err;
        }
        console.log("stdout: " + stdout);
        console.log("stderr: " + stderr);
        callback();
    });
});