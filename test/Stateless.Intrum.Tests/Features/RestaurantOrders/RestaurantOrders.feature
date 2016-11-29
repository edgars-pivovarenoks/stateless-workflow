Feature: Restaurant order handling
	In order to make customers happy
	As a reataurant manager
	I want to all employess follow the same procedure

@happypath
Scenario: Customer arrives to empty restaurant
	Given customer has arrived to restaurant
	And there are free tables available
	When waiter provides a table to the customer
	Then waiter starts to wait for order

@happypath
Scenario: Customer arrives to full restaurant
	Given customer has arrived to restaurant
	And there are no tables available
	When waiter rejects customer
	Then customer leaves restaurant