import asyncio
import httpx
import sys
import time

completed_requests = 0

async def execute_requests(session, url):
    global completed_requests
    while True:
        response = await session.get(url)
        completed_requests += 1

def print_results(requests, duration):
    requests_per_second = requests / duration
    print(f'Completed {requests:,} requests in {duration:.2f} seconds ({requests_per_second:,.0f} req/s)')
    print()

async def collect_results(title, duration):
    global completed_requests
    print(f'=== {title} ===')

    start = time.perf_counter()
    completed_requests = 0
    await asyncio.sleep(duration)
    end = time.perf_counter()

    print_results(completed_requests, end - start)

async def main():
    if len(sys.argv) == 1:
        print('Usage: app <url> <parallel> <warmup> <duration>')
        return
    
    url = sys.argv[1]
    parallel = int(sys.argv[2]) if len(sys.argv) >= 3 else 32
    warmup = int(sys.argv[3]) if len(sys.argv) >= 4 else 10
    duration = int(sys.argv[4]) if len(sys.argv) >= 5 else 10

    print('=== Parameters ===')
    print(f'Url: {url}')
    print(f'Parallel: {parallel}')
    print(f'Warmup: {warmup}')
    print(f'Duration: {duration}')
    print()

    async with httpx.AsyncClient() as session:
        asyncio.gather(*[execute_requests(session, url) for i in range(parallel)])
        await collect_results('Warmup', warmup)
        await collect_results('Test', duration)

loop = asyncio.get_event_loop()
loop.run_until_complete(main())
loop.close()
