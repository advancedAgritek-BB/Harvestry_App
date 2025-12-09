#!/bin/bash

# Update and Upgrade
echo "Updating system..."
sudo apt update && sudo apt upgrade -y

# Install Docker
echo "Installing Docker..."
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker ubuntu

# Install Docker Compose (included in newer Docker, but ensuring plugin)
sudo apt install docker-compose-plugin -y

# Install Git
echo "Installing Git..."
sudo apt install git -y

echo "Setup complete. Please logout and login again for Docker group changes to take effect."
