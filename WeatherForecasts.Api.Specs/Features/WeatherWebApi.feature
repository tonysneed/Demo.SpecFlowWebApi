Feature: Weather Forecast Web Api
	Web API for weather forecasts

Scenario: Get weather forecasts
	Given I am a client
	And the repository has weather data
	When I make a GET request to 'weatherforecast'
	Then the response status code is '200'
	And the response json should be 'weathers.json'

Scenario: Get weather forecast by id
	Given I am a client
	And the repository has weather data
	When I make a GET request with id '1' to 'weatherforecast'
	Then the response status code is '200'
	And the response json should be 'weather.json'

Scenario: Add weather forecast
	Given I am a client
	When I make a POST request with 'weather.json' to 'weatherforecast'
	Then the response status code is '201'
	And the location header is 'http://localhost/WeatherForecast/1'
	And the response json should be 'weather.json'

Scenario: Update weather forecast
	Given I am a client
	And the repository has weather data
	When I make a PUT request with 'weather.json' to 'weatherforecast'
	Then the response status code is '200'
	And the response entity should be 'weather.json'
	And it should have a new ETag
	
Scenario: Remove weather forecast
	Given I am a client
	And the repository has weather data
	When I make a DELETE request with id '1' to 'weatherforecast'
	Then the response status code is '204'