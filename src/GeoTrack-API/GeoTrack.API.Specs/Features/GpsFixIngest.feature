Feature: GPS fix ingest

  Background:
    Given the API is running
    And I have a valid API key
    And I have a tenant "11111111-1111-1111-1111-111111111111"
    And I have a vehicle "22222222-2222-2222-2222-222222222222" for that tenant

  Scenario: Ingesting a single GPS fix creates a gps fix and updates latest location
    When I POST a single GPS fix for vehicle "22222222-2222-2222-2222-222222222222" with device time "2026-01-30T12:00:00Z"
    Then the response status code should be 201
    And the vehicle "22222222-2222-2222-2222-222222222222" should have 1 gps fixes
    And the vehicle "22222222-2222-2222-2222-222222222222" latest location device time should be "2026-01-30T12:00:00Z"

  Scenario: Ingesting an older GPS fix does not override latest location
    Given the vehicle "22222222-2222-2222-2222-222222222222" has a latest location at "2026-01-30T12:10:00Z"
    When I POST a single GPS fix for vehicle "22222222-2222-2222-2222-222222222222" with device time "2026-01-30T12:00:00Z"
    Then the response status code should be 201
    And the vehicle "22222222-2222-2222-2222-222222222222" latest location device time should be "2026-01-30T12:10:00Z"

  Scenario: Batch ingest accepts valid items and rejects unknown vehicles
    When I POST a batch of 2 gps fixes where 1 is for an unknown vehicle
    Then the response status code should be 200
    And the batch ingest result should have 1 accepted and 1 rejected
