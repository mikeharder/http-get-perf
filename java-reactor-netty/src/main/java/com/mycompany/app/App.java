package com.mycompany.app;

import java.util.concurrent.atomic.AtomicInteger;

import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;
import reactor.core.scheduler.Schedulers;
import reactor.netty.http.client.HttpClient;

public class App {
    private static final HttpClient _httpClient = HttpClient.create();
    private static AtomicInteger _completedRequests = new AtomicInteger(0);

    public static void main(String[] args) {
        if (args.length == 0) {
            System.out.println("Usage: app <url> <parallel> <warmup> <duration>");
            return;
        }

        String url = args[0];
        int parallel = args.length >= 2 ? Integer.parseInt(args[1]) : 64;
        int warmup = args.length >= 3 ? Integer.parseInt(args[2]) : 30;
        int duration = args.length >= 4 ? Integer.parseInt(args[3]) : 10;

        System.out.println("=== Parameters ===");
        System.out.printf("Url: %s%n", url);
        System.out.printf("Parallel: %s%n", parallel);
        System.out.printf("Warmup: %s%n", warmup);
        System.out.printf("Duration: %s%n", duration);
        System.out.println();

        Flux.range(0, parallel).parallel().runOn(Schedulers.boundedElastic()).flatMap(i -> ExecuteRequests(url)).subscribe();

        CollectResults("Warmup", warmup);
        CollectResults("Test", duration);
    }

    private static Mono<Void> ExecuteRequests(String url) {
       return Flux.just(1).repeat()
                .flatMap(i -> _httpClient.get().uri(url).responseContent().aggregate().asString().then(Mono.just(1)), 1)
                .doOnNext(v -> {
                    _completedRequests.incrementAndGet();
                }).then();
    }

    private static void CollectResults(String title, int duration) {
        System.out.printf("=== %s ===%n", title);
        
        long startNanoTime = System.nanoTime();
        _completedRequests.set(0);

        try {
            Thread.sleep(duration * 1000);
        }
        catch (Exception e) {
        }

        PrintResults(_completedRequests.get(), (System.nanoTime() - startNanoTime));
    }

    private static void PrintResults(int completedRequests, long elapsedNanoTime) {
        double elapsedSeconds = elapsedNanoTime / 1000000000;
        double requestsPerSecond = completedRequests / elapsedSeconds;

        System.out.printf("Completed %d requests in %.2f seconds (%.0f req/s)%n",
            completedRequests, elapsedSeconds, requestsPerSecond);
        System.out.println();
    }
}
