# VeloCloud2Connectwise

### **1.**  **Introduction**

Console-based application that will pull companies from Velocloud to sync them with Connectwise via their respective APIs.  Sync is currently scheduled to be performed every 3 hours and can be modified via the environmental variables.

### **2.**  **Server Info**
Not currently in production

### **3.**  **Code Base**

The console application was written in .NET Core 2.1 and is configured to run in a Docker container.  Error & info messages are written to standard output.  Syncs are set to be performed every 3 hours and can be configured via the environment variable.  There is also a manual trigger that can be executed on port 8077.

### Manual Sync Trigger
The app is listening on port 8077.  You can manually trigger the application by navigating your browser or api call to http://[production_ip_address]:8077

### Metrics
Integrated with prometheus and listening on port 9700.  The following metrics are collected:
1) Total app tick counter [velocloud2connectwise_ticks_total]
2) Total sync jobs elapsed counter[velocloud2connectwise_job_elapsed]
3) Total sync error count [velocloud2connectwise_error]
4) Total customer accounts synced gauge [velocloud2connectwise_sync_total]
5) Total customer accounts collected [velocloud2connectwise_account_total]


### Environmental Variables
Environmental variables that are referenced by the application are configured in the docker-compose.override.yml file for deployment.  For local debugging outside of the docker container, you must set the environmental variables in the Visual Studio project properties/options in the Run configuration section.

The following variables are configured 
```
        - prometheusPort=1234
        - jobTimer=180
        - cwBaseURL=https://na.myconnectwise.net/v4_6_release/apis/3.0/
        - cwConsumerKey=XXX
        - cwConsumerSecret=XXX
        - cwClientID=XXX
        - vcoBaseURL=https://vco160-usca1.velocloud.net/portal/rest/
        - vcoConsumerKey=XXX
        - vcoConsumerSecret=XXX
        - vcoLoginURL=https://vco160-usca1.velocloud.net/magna5global/login/enterpriseLogin
        - emailReportTo=growe@magna5global.com

```

### Dependencies
1) .NET Core 2.1
2) Magna5 Utilities
3) Prometheus
4) Newtonsoft.Json




