# Mclaren Coding Test

## How to run the application
1. First, pull down the repo to a local directory. 
2. Open the repo with VS code
3. Open a terminal and run docker-compose pull
4. Once completed, docker-compose up
5. Open (http://localhost:8084)

## Assumptions
1. All message coming from the MQTT broker are in valid format and have passed validation.
2. To determine which car is in first place, I assume that the car that has travelled the most distance is leading the race.
