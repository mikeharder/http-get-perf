package com.mycompany.app;

import reactor.netty.http.client.HttpClient;

public class App {
    private static final HttpClient _httpClient = HttpClient.create();

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
        System.out.println(String.format("Url: %s", url));
        System.out.println(String.format("Parallel: %s", parallel));
        System.out.println(String.format("Warmup: %s", warmup));
        System.out.println(String.format("Duration: %s", duration));
        System.out.println();

        String response = _httpClient.get().uri(url).responseContent().aggregate().asString().block();

        System.out.println(response);
    }
}
