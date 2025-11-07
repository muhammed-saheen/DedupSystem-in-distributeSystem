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


=>>For testing purpose,used the websocketking client as event source<<=




