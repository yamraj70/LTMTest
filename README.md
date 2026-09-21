# NewRepo
This project implements a RESTful ASP.NET Core API that retrieves the best stories from the Hacker News API.

The caller specifies the number of stories (n). The application retrieves the available best-story IDs, gets the story details, sorts the stories by score in descending order, and returns the requested number of stories.

Requirements

.NET SDK compatible with the project target framework

Internet access to call the Hacker News API

Visual Studio or Visual Studio Code

Hacker News API

The application uses:

https://hacker-news.firebaseio.com/v0/beststories.json

https://hacker-news.firebaseio.com/v0/item/{storyId}.json

The base URL and endpoint paths are configured in appsettings.json.

Configuration

The appsettings.json file contains:

"HackerNewsApi": {
  "BaseUrl": "https://hacker-news.firebaseio.com/v0/",
  "BestStoriesEndpoint": "beststories.json",
  "ItemEndpoint": "item/{0}.json",
  "StoryFallbackUrl": "https://news.ycombinator.com/item?id={0}"
}

No Hacker News API key is required.

How to Run

1. Clone the repository

git clone NewRepo
cd NewRepo

2. Restore dependencies

dotnet restore

3. Build the application

dotnet build

4. Run the application

dotnet run

The application will display the local HTTP/HTTPS URLs in the console, for example:

https://localhost:5130
http://localhost:7108

Use one of the displayed URLs when calling the API.

API Usage

The endpoint is:

GET /api/Story?value={number}

For example:

GET /api/Story?value=10

This requests the best 10 stories.

Example using curl:

curl "https://localhost:NewRepo/api/Story?value=10"

Response

The API returns:

title

uri

postedBy

time

score

commentCount

Example:

[
  {
    "title": "Example story",
    "uri": "https://example.com/story",
    "postedBy": "username",
    "time": "2026-01-01T12:00:00Z",
    "score": 315,
    "commentCount": 121
  }
]

Stories are returned in descending order by score.

Validation

The n value must be greater than zero.

For example:

GET /api/Story?value=0

returns:

400 Bad Request

Implementation Details

Score-based selection

The service:

Retrieves story IDs from beststories.json.

Retrieves the details for those stories.

Sorts stories by score in descending order.

Takes the requested number of stories.

Maps the Hacker News model to the API response model.

The selection logic is:

var topStories = validStories
    .OrderByDescending(x => x.Score)
    .Take(count)
    .ToList();

Bounded concurrency

The application uses Parallel.ForEachAsync with:

MaxDegreeOfParallelism = 10

This limits the number of concurrent story-detail requests to Hacker News and avoids sending all requests simultaneously.

Caching

The application uses IMemoryCache.

Best story IDs are cached for 30 seconds.

Individual story details are cached for 10 minutes.

Caching reduces repeated calls to Hacker News when the same data is requested again.
Assumptions

The Hacker News API is available and returns the expected response format.

The IDs returned by beststories.json are the stories evaluated for the requested result.

If an individual story request fails, that story is skipped and the remaining stories continue to be processed.

If fewer valid stories can be retrieved than requested because of upstream failures, the API returns the successfully retrieved stories.

IMemoryCache is local to the running application instance.

Error Handling

The application handles:

Invalid n values

HTTP request failures

Request timeouts/cancellation

Unexpected exceptions

An individual story failure does not stop the retrieval of the remaining stories.