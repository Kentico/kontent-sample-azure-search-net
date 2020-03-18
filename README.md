# Kentico Kontent Sample: Azure Cognitive Search Integration

This is a sample .NET Core 3.1 web application with a basic integration of Kentico Kontent with Azure Cognitive Search. It will handle creating, initializing, updating, and searching an index for the articles in the Dancing Goat sample project.

## Requirements

* .NET Core 3.1
* Visual Studio 2019 (or later)
* Dancing Goat sample project in Kontent
* Azure Cognitive Search service

## Basic Setup

1. Fork the project and open in Visual Studio
1. Get your Kentico Kontent project ID
    1. In Kentico Kontent, choose a project.
    1. From the app menu, choose Project settings.
    1. Under Development, choose API keys.
    1. In the Secure access box, click Copy to clipboard.
1. Make sure you've provisioned an Azure Cognitive Search service, you'll need the following from it:
    1. The service name (e.g. my-azure-search)
    1. An admin key
1. In `appsettings.json` or using the user `secrets.json` feature of Visual Studio add the following keys and update the values with your values (you can ignore the webhook secret for now):

    ```json
    {
      "SearchServiceName": "your-azure-search-service-name",
      "SearchServiceAdminApiKey": "YourAdminApiKey",
      "SearchIndexName": "desired-index-name",
      "KontentProjectID": "kontent-project-id",
      "KontentWebhookSecret": "WebhookSecretValue"
    }
    ```

1. Hit the "debug" button to run it in debug mode
1. Change the url to "/search"
1. Click "Create index"
1. Click "Initialize index"
1. Enter "coffee" in the search box and click "search"

## Webhook setup

If you want to set up automatic updates, you'll need to set up a webhook. Here's how you can set this up and test it locally:

1. Download [ngrok](https://ngrok.com/) and follow [their instructions to get started](https://dashboard.ngrok.com/get-started)
1. When you're ready to fire it up run the following command (updating the port as necessary):

    ```console
    ./ngrok http https://localhost:44345 -host-header="localhost:44345"
    ```

1. Add a new webhook in Kentico Kontent:
    1. Go to "Settings" then "Webhooks"
    1. Click "Create new Webhook" button
    1. Enter the ngrok "forwarding" URL
    1. Confirm that the "publish" and "unpublish" delivery API triggers are enabled
    1. Confirm that no other triggers are enabled
    1. Copy the webhook secret to the `KenticoWebhookSecret` setting in `appsettings.json` or the user `secrets.json`
