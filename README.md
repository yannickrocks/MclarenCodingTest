# Mclaren Coding Test

## Prerequisites:

* [docker](https://docs.docker.com/)
* [docker-compose](https://docs.docker.com/compose/)

## How to run the application
1. First, pull down the repo to a local directory. 
2. Open the repo with an IDE (If you have VS code, that is the preferable) and/or open a terminal 
3. Run docker-compose pull
4. Once completed, docker-compose up
5. Open (http://localhost:8084)

## Assumptions
1. All message coming from the MQTT broker are in valid format and have passed validation.
2. To determine which car is in first place, I assume that the car that has travelled the most distance is leading the race.
