# HTTP GET performance comparison

Client was run on Azure DS1_V2 (1 core) for parity since Python and JS are limited to a single CPU.  Server was run on Azure DS3_V2 (4 cores) to ensure server was not the bottleneck.

![image](https://user-images.githubusercontent.com/9459391/75201357-1c60de80-571d-11ea-8a29-a33f6eee1059.png)

| Client              | Description                       | Connections | Requests Per Second |
|---------------------|-----------------------------------|-------------|---------------------|
| wrk                 | Benchmarking tool (written in C)  | 256         | 71,284              |
| bombardier          | Benchmarking tool (written in go) | 256         | 62,988              |
| net-sockets         | .NET Core Async Sockets           | 64          | 67,231              |
| net-http-client     | .NET Core Async HttpClient        | 64          | 28,099              |
| java-reactor-netty  | Java Reactor Netty                | 64          | 26,546              |
| js-sockets          | JavaScript net.Socket             | 128         | 64,795              |
| js-http             | JavaScript http                   | 64          | 13,971              |
| js-node-fetch       | JavaScript node-fetch             | 64          | 8,430               |
| python-sockets      | Python Async Sockets              | 256         | 33,652              |
| python-aiohttp      | Python Async AioHttp              | 32          | 2,135               |

`wrk` is a benchmarking tool written in C and designed for maximum throughput.  It represents the theoretical maximum performance of any language's HTTP client implementation.

`bombardier` is an alternative benchmarking tool written in Go.

`net-sockets`, `js-sockets`, and `python-raw-sockets` use raw sockets to send an HTTP GET message and read the response bytes.  The request message bytes are pre-computed and reused for every request, and the response bytes are read but not parsed.  This represents the theoretical maximum performance of a given language's HTTP client implementation.  (`python-sockets` uses Python's async socket interface from the stdlib's `asyncio` library.)

`net-http-client` uses the `System.Net.HttpClient` client.

`java-reactor-netty` uses the `reactor-netty` client.

`js-http` uses the `http` client, and `js-node-fetch` uses the `node-fetch` client.

`python-aiohttp` uses the `aiohttp` client.

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
3. `docker compose run --rm server`
4. The same instance of the server can be re-used for all test runs

## Client
The client apps make HTTP GET requests to the server in a loop and measure throughput.  Example:

```
~/http-get-perf/python-aiohttp$ docker-compose run --rm client http://server-host-or-ip:5000
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
3. `docker compose run --rm client http://<server-host-or-ip>:5000`
