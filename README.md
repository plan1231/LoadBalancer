# Load Balancer
This repository contains an implementation of a load balancer based on a pipeline of middlewares for ASP.NET Core applications.
The design is heavily inspired by Microsoft's YARP. Two source code files, `AtomicCounter.cs` and `RoundRobinPolicy.cs`, are closely based off of their counterparts in the YARP repo.

## Features
- Load Balancing Policies: The middleware supports adding load balancing policies to distribute the incoming requests among available backend services.
Right now, only round robin is supported. To add more, add a new class that implements `ILoadBalancingPolicy`
- Session Affinity: The middleware provides session affinity support using a simple cookie-based policy, which binds client requests to specific backend services.
Right now, only a simple cookie-based session affinity policy is supported. This policy is for demonstration purposes only and should never be used in production, as it stores the selected endpoint url in plaintext.
One could easily port this to production by adding encryption/decryption.
To implement new policies, add a new class that implements `ISessionAffinityPolicy`
- Health Check: The middleware can be configured to route requests only to healthy backend services. The health status of backend services is maintained by a separate service.

## Configuration

To configure the load balancer middleware, you need to define the required settings in your appsettings.json file and load them into your application during startup.


Add a LoadBalancer section to your `appsettings.json` file with the following properties:

```json
{
  "LoadBalancer": {
    "HealthCheck": {
      "Enabled": true,
      "Interval": 30,
      "Timeout": 5
    },
    "SessionAffinity": {
      "Enabled": true,
      "Policy": "SimpleCookie",
      "AffinityKeyName": "AffinityKey"
    },
    "Policy": "RoundRobin",
    "Endpoints": [
      "https://backend-service-1.example.com",
      "https://backend-service-2.example.com"
    ]
  }
}
```
property key:

- `HealthCheck`: Configures the health check settings.
    - `Enabled`: Enables or disables the health check feature (boolean).
    - `Interval`: Sets the interval between health checks (seconds).
    - `Timeout`: Sets the timeout for a health check request (seconds).
- `SessionAffinity`: Configures the session affinity settings.
    - `Enabled`: Enables or disables session affinity (boolean).
    - `Policy`: Sets the session affinity policy to use (string). Right now, the only supported value iis `'SimpleCookie'`
    - `AffinityKeyName`: Sets the affinity key name used by the session affinity policy (string).
- `Policy`: Sets the load balancing policy to use (string). Right now, the only supported value is `'RoundRobin'`
- `Endpoints`: A list of backend service endpoints (array of strings).

## Running
There are two options to running the project

### Option 1
Use Visual Studio and open the LoadBalancer solution.  Configure the appsettings.json file as desired. Build the project LoadBalancer using the UI. Visual studio should handle copying appsettings.json to the build directory for you

### Option 2
Use the CLI. At the solution root, run `dotnet build LoadBalancer/LoadBalancer.csproj`. The compiler should have already copy appsettings.json into the output folder for you. If not, copy it manually from `LoadBalancer/appsettings.json`.
Then edit it according to the key above.

To find where the output directory is, simply look at the output of the build command.
```
phillan@Phils-MacBook-Pro LoadBalancer % dotnet build LoadBalancer/LoadBalancer.csproj

MSBuild version 17.5.1+f6fdcf537 for .NET
  Determining projects to restore...
  All projects are up-to-date for restore.
  LoadBalancer -> /Users/phillan/Projects/LoadBalancer/LoadBalancer/bin/Debug/net7.0/LoadBalancer.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.24
```
Then `cd` into the output directory and run `dotnet LoadBalancer.dll`


## Limitations
- No ssl support.
- Classes in namespace `LoadBalancer.Configuration` conflict with other namespaces, which means every time you want to use them you have to use add `Configuration.` before the class name. I made this trade-off conciously as this makes
the code to parse the config file a lot shorter, while also making the json field names more user friendly (e.g. in the config file, it's "SessionAffinity" instead of "SessionAffinityConfig")