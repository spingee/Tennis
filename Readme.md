# Tennis Simulator Application (Hiring task)

This is a Tennis Simulator application that allows you to start a new match and get the match result. The application is built using C# and Microsoft Orleans for persistent queue. Communicating between match and players is done by persistent Azure queue. If application is stopped (by stopping docker container) while there are active matches they should be resumed after restarting container again. It is possible to run infinite number of matches in parallel. It is possible to observe live log of running matches in container console. 

## Technology Stack

- C#
- Microsoft Orleans
- Docker
- ASP.NET Core

## Running the Application

To run the application, you need to have Docker installed on your machine. Once Docker is installed, navigate to the root directory of the project and run the following command:

```bash
docker-compose up
```

This will start the application. You can then access it at `http://localhost:5000`.

## Swagger UI

The application includes a Swagger UI that provides a visual interface for interacting with the API. Once the application is running, you can access the Swagger UI at `http://localhost:5000/swagger`.

## Example Requests

### Start a Match

To start a new match, make a POST request to the `/match/{name}` endpoint. Replace `{name}` with the name of the match. The request body should contain the experience levels of the two players.

```json
{
  "ExperiencePlayer1": 45,
  "ExperiencePlayer2": 79
}
```

### Get Match Result

To get the result of a match, make a GET request to the `/match/{name}` endpoint. Replace `{name}` with the name of the match.