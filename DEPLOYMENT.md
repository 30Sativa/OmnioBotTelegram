# Deployment Guide

## Environment Variables

For deployment, you need to set the following environment variable:

- `BOT_TOKEN`: Your Telegram bot token from @BotFather

## Render Deployment

1. Connect your repository to Render
2. Set the environment variable `BOT_TOKEN` in your Render service settings
3. The Docker build will now work correctly with the updated configuration

## Local Development

For local development, you can either:

1. Set the `BOT_TOKEN` environment variable
2. Or create a local `appsettings.json` file with your bot token

## Docker Build

The Dockerfile has been updated to handle the missing `appsettings.json` file. The application will now:

1. Look for `appsettings.json` in the project directory
2. Fall back to environment variables if the file doesn't exist
3. Use the `BOT_TOKEN` environment variable for the bot token

## Security Note

The `appsettings.json` file is now included in the repository but uses environment variable substitution. For production, always use environment variables to set the bot token. 