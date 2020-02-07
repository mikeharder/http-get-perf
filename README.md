# HTTP GET performance comparison

This repo contains performance tests for a "hello world" HTTP GET request.

# Results

Tests were run on Azure DS3_v2 VMs.  Client app was limited to a single CPU via `docker run --cpus 1` for parity since Python is limited to a single CPU.

| Client          | Description                    | Requests Per Second |
|-----------------|--------------------------------|---------------------|
| wrk             | Benchmarking tool written in C | 41,073              |
| net-sockets     | .NET Core Async Sockets        | 28,942              |
| net-http-client | .NET Core Async HttpClient     | 14,026              |
| python-socket   | Python Async Sockets           | 26,476              |
| python-aiohttp  | Python Async AioHttp Client    |  1,701              |

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
