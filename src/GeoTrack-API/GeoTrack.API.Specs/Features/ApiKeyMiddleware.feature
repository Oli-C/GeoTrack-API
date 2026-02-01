Feature: API key middleware

  Background:
    Given the API is running

  Scenario: Health endpoint is anonymous
    When I GET "/health"
    Then the response status code should be 200

  Scenario: Vehicles endpoint requires an API key
    When I GET "/vehicles"
    Then the response status code should be 401

  Scenario: Vehicles endpoint accepts a valid API key but still requires a tenant header
    Given I have a valid API key
    When I GET "/vehicles"
    Then the response status code should be 400
