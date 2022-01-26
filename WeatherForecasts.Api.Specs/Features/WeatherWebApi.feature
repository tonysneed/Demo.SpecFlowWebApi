Feature: Weather Forecast Web Api
	Web API for weather forecasts

@mytag
Scenario: Get weather forecasts
	Given I am a client
	When I make a GET request to 'weatherforecast'
	Then the response status code is '200'
	And the response data should be 'weathers.json'
