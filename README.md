# Mclaren Coding Test

## How to run the application
First, pull down the repo to a local directory. Then there are several ways you can run the application. These include:
1. Within the MclarenTest folder, open up a git bash and run the following commands:
  - dotnet build
  - dotnet publish
Then within the netcoreapp3.1 folder (Mqttservice\bin\Debug\netcoreapp3.1) is an exe file called MqttService.exe. Double click this file and that will start the service. 

**OR**

2. Open the Solution and press play. This will start the service.

Then run the Mat-Coding-Challenge code with "docker-compose up"

**It is important that you run the service before you run the MqttBroker/WebApp or else it will miss out on starting events**

## Assumptions
1. All message coming from the MQTT broker are in valid format and have passed validation.
2. To determine which car is in first place, I assume that the car that has travelled the most distance is leading the race.
