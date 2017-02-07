## Sitecore 8.2 Deployment Automation

### Introduction
The Deployment Automation is using the Automation Framework to perform deployment of Sitecore 8.2 onto the target computer. The configuration of the deployment is achieved through the usage of a deployment configuration file, a set of predefine actions and action groups to perform a set of actions in one single command.

In the case of the current deployment, the configuration file is "Sitecore82.deployment.json". It was derived from the "ScPerf82.deployment.json" located in the "C:\SitecorePowerShell\DeploymentConfigs\DeploymentTemplates" folder. Some changes regarding the versions of the product being installed were updated to reflect the current development state.

### Prerequisites
In order to use the Automation Deployment you need to install the Automation Framework on your computer. This is easily achieve by running a Chocolatey install.

#### Install Chocolatey
If you don't have Chocolatey installed on your machine, run the following command from an Administrator command prompt:

    iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))

#### Install ps.ads
The Automation Framework uses the ps.ads package distributed by DevOps. To install it, run the command below. If a newer version is available, it should be compatible with the version in use at the time of the writing of this document. So please verify that you are installing the latest version of ps.ads.

    choco install ps.ads -Version 1.6.966 -Source http://nuget1dk1:8181/nuget/Packages

#### Install the Automation Framework
To install the Automation Framework using Chocolatey, run the following command:

    choco install PsAutomationFramework -source “http://nuget1ca2/nuget/Packages” -y

#### Upgrade the Automation Framework
To upgrade the Automation Framework, run the following command:

    choco upgrade PsAutomationFramework -source “http://nuget1ca2/nuget/Packages” -y

### Usage
To perform the deployment, use one of the following PS1 script files provided. Basically, the script contained in the file loads the deployment configuration and execute the "Install-Deployment" method provided by the Automation Framework to execute the configured actions.

To perform a full deployment (Commerce Server and Sitecore 8.2):

    FullDeploymentStorefront_CS_8.2.ps1

To perform a full cleanup (Commerce Server and Sitecore 8.2):

    CleanStorefrontDeployment_CS_8.2.ps1

To perform a Commerce Server only install:

    DeployCommerceServerOnly_CS_8.2.ps1

To perform a Commerce Server only cleanup:

    CleanCommerceServerOnly_CS_8.2.ps1

To perform a Sitecore only install:

    DeployStorefrontOnly_CS_8.2 .ps1

To perform a Sitecore only cleanup:

    CleanStorefrontOnly_CS_8.2.ps1

### Testing and Debug
Testing is performed through the usage of the Cheat Sheet. After loading the configuration JSON file into the configuration variable, you can execute the configured tasks step by step to confirm that the script is performing as expected.

To debug the Sitecore 8.2 Deployment Automation, again use the Cheat Sheet in order to step through the deployment process. We recommend you use ISE to perform debug session since you can set breakpoints during the execution of the scripts. A good package to install on ISE is the "Package Explorer". You can load all packages in the "C:\SitecorePowerShell" folder and find functions, peek at the code and set breakpoints to investigate issues.  

### Testing Deployed Site
##### To navigate to the administration login page, use the following URL:

    http://localhost:7667/sitecore/login


##### To navigate to the Commerce Server site, use the following URL:

    http://your_machine_name/
Note: This is the same URL that QA is mapping to, feel free to add new host entries if you wish.

### Contribution
Please refer to the [CONTRIBUTION.md](http://nuget1ca2/feeds/Packages/CONTRIBUTION.md).

### Samples
Sample configuration files (x.deployment.json) are available in the C:\SitecorePowerShell\DeploymentConfigs\DeploymentTemplates folder
