# HTTP GET performance comparison

Client was run on Azure DS1_V2 (1 core) for parity since Python is limited to a single CPU.  Server was run on Azure DS3_V2 (4 cores) to ensure server was not the bottleneck.

![image](https://user-images.githubusercontent.com/9459391/75082146-17a2ed00-54c7-11ea-858c-0bd42b531b7b.png)

| Client           | Description                       | Connections | Requests Per Second |
|------------------|-----------------------------------|-------------|---------------------|
| wrk              | Benchmarking tool (written in C)  | 256         | 71,284              |
| bombardier       | Benchmarking tool (written in go) | 256         | 62,988              |
| net-sockets      | .NET Core Async Sockets           | 64          | 67,231              |
| net-http-client  | .NET Core Async HttpClient        | 64          | 28,099              |
| python-sockets   | Python Async Sockets              | 256         | 33,652              |
| python-aiohttp   | Python Async AioHttp              | 32          | 2,135               |
| js-http          | JavaScript http                   | 64          | 13,971              |
| js-node-fetch    | JavaScript node-fetch             | 64          | 8,430               |

`wrk` is a benchmarking tool written in C and designed for maximum throughput.  It represents the theoretical maximum performance of any language's HTTP client implementation.

`bombardier` is an alternative benchmarking tool written in Go.

`net-sockets` and `python-sockets` use raw sockets to send an HTTP GET message and read the response bytes.  The request message bytes are pre-computed and reused for every request, and the response bytes are read but not parsed.  This represents the theoretical maximum performance of a given language's HTTP client implementation.

`net-http-client` uses the `System.Net.HttpClient` client, and `python-aiohttp` uses the `aiohttp` client.

`js-http` uses the `http` client, and `js-node-fetch` uses the `node-fetch` client.

# Repro Steps

For the most accurate results, the server and client apps should be run on separate machines.

## Server
The server is a tiny ASP.NET Core application which listens on port 5000 and reponds to all requests with the following response:

```
HTTP/1.1 200 OK                                            
Date: Fri, 07 Feb 2020 20:15:32 GMT                        
Content-Type: text/plain                                   
Server: Kestrel                                            
Content-Length: 13                                         
                                                
Hello, World!
```

1. `git clone https://github.com/mikeharder/http-get-perf`
2. `cd http-get-perf/server`
3. `./build.sh`
4. `./run.sh`
5. The same instance of the server can be re-used for all test runs

## Client
The client apps make HTTP GET requests to the server in a loop and measure throughput.  Example:

```
~/http-get-perf/python-aiohttp$ ./run.sh http://server-host-or-ip:5000
=== Parameters ===
Url: http://server-host-or-ip:5000
Parallel: 32
Warmup: 10
Duration: 10

=== Warmup ===
Completed 15,680 requests in 10.03 seconds (1,564 req/s)

=== Test ===
Completed 17,055 requests in 10.03 seconds (1,701 req/s)
```

1. `git clone https://github.com/mikeharder/http-get-perf`
2. `cd http-get-perf/<client-app>`
3. `./build.sh`
4. `./run.sh http://<server-host-or-ip>:5000`
