module.exports = function() {
  var instanceRoot = "C:\\websites\\habitat.dev.local";
  var config = {
    websiteRoot: instanceRoot + "\\Website",
    sitecoreLibraries: instanceRoot + "\\Website\\bin",
    licensePath: instanceRoot + "\\Data\\license.xml",
    solutionName: "Sitecore.Demo.Retail",
    buildConfiguration: "Debug",
    runCleanBuilds: false,
    commerceServerSiteName: "Habitat",
    commerceDatabasePath: ".\\src\\Project\\Retail\\Database"
  };
  return config;
}