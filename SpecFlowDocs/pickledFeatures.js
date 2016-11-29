jsonPWrapper ({
  "Features": [
    {
      "RelativeFolder": "RestaurantOrders\\RestaurantOrders.feature",
      "Feature": {
        "Name": "Restaurant order handling",
        "Description": "In order to make customers happy\r\nAs a reataurant manager\r\nI want to all employess follow the same procedure",
        "FeatureElements": [
          {
            "Name": "Customer arrives to empty restaurant",
            "Slug": "customer-arrives-to-empty-restaurant",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "customer has arrived to restaurant",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "there are free tables available",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "waiter provides a table to the customer",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "waiter starts to wait for order",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@happypath"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false
            }
          },
          {
            "Name": "Customer arrives to full restaurant",
            "Slug": "customer-arrives-to-full-restaurant",
            "Description": "",
            "Steps": [
              {
                "Keyword": "Given",
                "NativeKeyword": "Given ",
                "Name": "customer has arrived to restaurant",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "And",
                "NativeKeyword": "And ",
                "Name": "there are no tables available",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "When",
                "NativeKeyword": "When ",
                "Name": "waiter rejects customer",
                "StepComments": [],
                "AfterLastStepComments": []
              },
              {
                "Keyword": "Then",
                "NativeKeyword": "Then ",
                "Name": "customer leaves restaurant",
                "StepComments": [],
                "AfterLastStepComments": []
              }
            ],
            "Tags": [
              "@happypath"
            ],
            "Result": {
              "WasExecuted": false,
              "WasSuccessful": false
            }
          }
        ],
        "Result": {
          "WasExecuted": false,
          "WasSuccessful": false
        },
        "Tags": []
      },
      "Result": {
        "WasExecuted": false,
        "WasSuccessful": false
      }
    }
  ],
  "Summary": {
    "Tags": [
      {
        "Tag": "@happypath",
        "Total": 2,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 2
      }
    ],
    "Folders": [
      {
        "Folder": "RestaurantOrders",
        "Total": 2,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 2
      }
    ],
    "NotTestedFolders": [
      {
        "Folder": "RestaurantOrders",
        "Total": 0,
        "Passing": 0,
        "Failing": 0,
        "Inconclusive": 0
      }
    ],
    "Scenarios": {
      "Total": 2,
      "Passing": 0,
      "Failing": 0,
      "Inconclusive": 2
    },
    "Features": {
      "Total": 1,
      "Passing": 0,
      "Failing": 0,
      "Inconclusive": 1
    }
  },
  "Configuration": {
    "GeneratedOn": "11 novembrī 2016 17:51:42"
  }
});