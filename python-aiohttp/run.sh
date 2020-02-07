#!/usr/bin/env bash

docker run -it --rm --network host --cpus 1 http-get-perf-python-aiohttp "$@"
