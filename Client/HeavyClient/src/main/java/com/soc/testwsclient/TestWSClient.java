package com.soc.testwsclient;

import com.baeldung.soap.ws.client.generated.ArrayOfInstruction;
import com.baeldung.soap.ws.client.generated.Instruction;
import com.baeldung.soap.ws.client.generated.Itinerary;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.xml.bind.JAXBElement;
import org.apache.activemq.transport.stomp.StompConnection;
import org.apache.activemq.transport.stomp.StompFrame;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.*;

public class TestWSClient {

    private static final HttpClient client = HttpClient.newHttpClient();
    private static final ObjectMapper objectMapper = new ObjectMapper();

    private static final String BROKER_URL = "tcp://localhost";
    private static final int BROKER_PORT = 61616;
    private static final String LOGIN = "admin";
    private static final String PASSCODE = "admin";

    private static final List<Itinerary> receivedItineraries = new ArrayList<>();

    public static void main(String[] args) {
        System.out.println("Welcome to Let's Go Biking\n");

        // Start a thread to listen to messages from ActiveMQ
        new Thread(TestWSClient::subscribeToQueue).start();

        // Main menu for user actions
        List<String> actions = Arrays.asList("Plan a journey", "Exit");
        performAction(actions, "Choose Action", new ArrayList<>());
    }

    private static void performAction(List<String> actions, String action, List<String> target) {
        Scanner scanner = new Scanner(System.in);

        switch (action) {
            case "Choose Action":
                System.out.println("\nChoosing an action ...\n");
                printOptions(actions);
                int index = chooseOption(actions.size());
                String chosenAction = actions.get(index);

                performAction(actions, chosenAction, target);
                break;

            case "Plan a journey":
                System.out.println("\nPlanning a journey ...\n");

                System.out.print("\nEnter the origin address :\nAddress : ");
                String startAddress = scanner.nextLine();

                System.out.print("\nEnter the destination address :\nAddress : ");
                String endAddress = scanner.nextLine();

                while (startAddress.equals(endAddress)) {
                    System.out.print("\nPlease, choose a different destination than the origin :\nAddress : ");
                    endAddress = scanner.nextLine();
                }

                // Resolve addresses and request itinerary
                requestItinerary(startAddress, endAddress);

                // Wait for the itineraries to be received from the queue
                System.out.println("\nWaiting for itineraries...\n");
                try {
                    Thread.sleep(3000); // Simulate wait time
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt();
                }

                // Display received itineraries
                printJourney(receivedItineraries);


                actions = Arrays.asList("Plan a journey", "Exit");
                performAction(actions, "Choose Action", target);
                break;

            case "Exit":
                System.exit(0);
                break;
        }
    }

    private static void printOptions(List<String> names) {
        for (int i = 0; i < names.size(); i++) {
            System.out.println((i + 1) + ". " + names.get(i));
        }
    }

    private static int chooseOption(int size) {
        Scanner scanner = new Scanner(System.in);
        int choice = -1;

        System.out.print("\nPlease, choose an action :\nNumber : ");
        while (choice < 1 || choice > size) {
            try {
                choice = Integer.parseInt(scanner.nextLine());
                if (choice < 1 || choice > size) {
                    System.out.print("\nNot a valid index. Please, retry\nNumber : ");
                }
            } catch (NumberFormatException e) {
                System.out.print("\nNot a valid number. Please, retry\nNumber : ");
            }
        }
        return choice - 1;
    }

    private static void printJourney(List<Itinerary> itineraries) {
        if (itineraries.isEmpty()) {
            System.out.println("\nNo journey found for the given addresses\n");
        } else {
            System.out.println("\nHere are some suggested itineraries :\n");
            for (Itinerary itinerary : itineraries) {
                System.out.println("Distance : " + itinerary.getDistance());
                System.out.println("Duration : " + itinerary.getDuration());
                System.out.println("Steps :");

                JAXBElement<ArrayOfInstruction> instructionsElement = itinerary.getInstructions();
                if (instructionsElement != null && instructionsElement.getValue() != null) {
                    ArrayOfInstruction instructions = instructionsElement.getValue();
                    for (Instruction instruction : instructions.getInstruction()) {
                        System.out.println(instruction.getText());
                    }
                } else {
                    System.out.println("No instructions available.");
                }
            }
        }
    }

    public static void subscribeToQueue() {
        StompConnection connection = new StompConnection();

        try {
            connection.open(BROKER_URL, BROKER_PORT);
            connection.connect(LOGIN, PASSCODE);

            System.out.println("[ActiveMQ] Connecté au broker via STOMP");

            // Abonnement à la file d'attente
            connection.subscribe("/queue/ItinerarySuggested-12345", "auto");
            System.out.println("[ActiveMQ] Abonné à la file d'attente");

            // Traitement des messages
            while (true) {
                System.out.println("[ActiveMQ] En attente d'un message...");
                // Utilisez receiveFrame pour récupérer le message sous forme de StompFrame
                StompFrame frame = connection.receive();
                if (frame != null) {
                    String body = frame.getBody();
                    System.out.println("[ActiveMQ] Message reçu : " + body);

                    // Traitez le message reçu (par exemple, désérialisation JSON)
                    handleMessage(body);
                }
            }
        } catch (Exception e) {
            System.err.println("[ActiveMQ] Erreur : " + e.getMessage());
        } finally {
            try {
                connection.disconnect();
            } catch (Exception e) {
                System.err.println("[ActiveMQ] Erreur lors de la déconnexion : " + e.getMessage());
            }
        }
    }


    private static void handleMessage(String body) {
        try {
            Itinerary itinerary = objectMapper.readValue(body, Itinerary.class);
            synchronized (receivedItineraries) {
                receivedItineraries.add(itinerary);
            }
        } catch (Exception e) {
            System.err.println("[ActiveMQ] Error deserializing the message: " + e.getMessage());
        }
    }

    private static void requestItinerary(String startAddress, String endAddress) {
        Map<String, Double> startLocation = resolveAddress(startAddress);
        Map<String, Double> endLocation = resolveAddress(endAddress);

        if (startLocation != null && endLocation != null) {
            System.out.println("[Request] Itinerary requested from " + startAddress + " to " + endAddress);
            // TODO: Send a request to the server (e.g., via REST API or SOAP)
        } else {
            System.out.println("[Request] Failed to resolve addresses.");
        }
    }

    public static Map<String, Double> resolveAddress(String address) {
        String escapedAddress = java.net.URLEncoder.encode(address, java.nio.charset.StandardCharsets.UTF_8);
        String url = "https://nominatim.openstreetmap.org/search?q=" + escapedAddress + "&format=json";

        try {
            HttpRequest request = HttpRequest.newBuilder()
                    .uri(URI.create(url))
                    .header("User-Agent", "HTTP_Client/1.0 (sagesseadabadji@gmail.com)")
                    .GET()
                    .build();

            HttpResponse<String> response = client.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                System.out.println("\nError: HTTP response code " + response.statusCode());
                return null;
            }

            JsonNode results = objectMapper.readTree(response.body());

            if (results.isEmpty()) {
                System.out.println("\nNo results found for address");
                return null;
            }

            JsonNode firstResult = results.get(0);
            return Map.of(
                    "lat", firstResult.get("lat").asDouble(),
                    "lon", firstResult.get("lon").asDouble()
            );
        } catch (Exception ex) {
            System.err.println("\nError resolving address\n" + ex.getMessage());
            return null;
        }
    }
}
