Distributed Event Deduplication System

📘 Overview

This project demonstrates a distributed, fault-tolerant event processing system designed to handle duplicate WebSocket events across multiple instances in a scalable environment.
The system ensures that each event is processed and persisted exactly once, even when multiple listener instances receive the same message.

🏗️ Architecture
  
Tech Stack

  =>.NET 8 / C# — Application and WebSocket handling
  =>RabbitMQ — Message queue for distributed event delivery
  =>MySQL — Event persistence and deduplication store
  =>Kubernetes (Minikube) — Container orchestration and scaling

Docker — Containerization of all services

⚙️ Core Components

🧩 1. WebSocket Listener & Publisher
  Listens for incoming WebSocket events.
  Publishes received messages to RabbitMQ (event_queue).
  Designed to simulate real-world event broadcasting.

🧩 2. RabbitMQ Queue
  Acts as a central broker for incoming events.
  Multiple consumers (pods) listen to the same queue.
  Ensures messages are delivered to only one consumer (exactly-once processing).

🧩 3. Consumer & Deduplication Logic
  Each consumer checks MySQL before processing:
  If event already processed → skip.
  If new → process, persist, and mark as completed.
  Prevents duplicate processing even across multiple replicas.

🧩 4. MySQL Database
  Stores processed event IDs.
  Ensures event persistence and idempotency in distributed setups.

🗂️ Project Structure
  /DeduplicationOfDistributedSystem
  │
  ├── Services/
  │   ├── WebSocketPublisher.cs      # Publishes WebSocket events to RabbitMQ
  │   ├── EventConsumer.cs           # Consumes events & applies deduplication
  │
  ├── appsettings.json               # Default config for local environment
  ├── Dockerfile                     # Container build file          
  │
  └── README.md                      # Documentation file


   How to run locally    

1) Prerequisites

  Make sure you have the following installed:
  =>RabbitMQ
   (running locally on port 5672)
  =>MySQL Server
   (running locally on port 3306)

2) Run the Application
From the project directory, run:
  $=>dotnet run

  You’ll see console output like:
  Now listening on: https://localhost:7254
  WebSocket endpoint: /ws

3) connect Using WebSocket King
  =>Open WebSocket King
  =>Connect to your local WebSocket endpoint:
  =>ws://localhost:7254/ws
  =>Once connected, send a test message:
  
  {
    "EventId": "evt_1001",
    "Payload": "Test event from local WebSocketKing"
  }


You’ll see the console log:

📨 Received: {"EventId":"evt_1001","Payload":"Test event from local WebSocketKing"}
📤 Published event: evt_1001
✅ Processed event evt_1001

5️⃣ Verify Deduplication

Try sending the same event ID multiple times:

{
  "EventId": "evt_1001",
  "Payload": "Duplicate event test"
}

You’ll see:

⚠️ Duplicate event evt_1001, skipping...
   This confirms the deduplication logic is working locally.

6️⃣ Event Flow Summary
WebSocketKing → WebSocket Listener → RabbitMQ Queue → EventConsumer → MySQL Database






