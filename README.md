# HTTP GET performance comparison

Tests were run on Azure DS3_v2 VMs.  Client app was limited to a single CPU via `docker run --cpus 1` for parity since Python is limited to a single CPU.

![image](https://user-images.githubusercontent.com/9459391/74974414-0f20b880-53da-11ea-9a66-3f2f48fdf318.png)

| Client           | Description                       | Requests Per Second |
|------------------|-----------------------------------|---------------------|
| bombardier       | Benchmarking tool (written in go) | 67,390              |
| wrk              | Benchmarking tool (written in C)  | 42,283              |
| net-sockets      | .NET Core Async Sockets           | 28,942              |
| net-http-client  | .NET Core Async HttpClient        | 14,026              |
| python-sockets   | Python Async Sockets              | 26,476              |
| python-aiohttp   | Python Async AioHttp Client       |  1,701              |

`bombardier` and `wrk` are benchmarking tools written in Go and C and designed for maximum throughput.  This represents the theoretical maximum performance of any language's HTTP client implementation.

`net-sockets` and `python-sockets` use raw sockets to send an HTTP GET message and read the response bytes.  The request message bytes are pre-computed and reused for every request, and the response bytes are read but not parsed.  This represents the theoretical maximum performance of a given language's HTTP client implementation.

`net-http-client` uses the `System.Net.HttpClient` client, and `python-aiohttp` uses the `aiohttp` client.

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
