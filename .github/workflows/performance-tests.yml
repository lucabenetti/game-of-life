name: Performance Tests

on:
  push:
    branches:
      - main
      - develop
  pull_request:
    branches:
      - main
      - develop

jobs:
  stress-test:
    runs-on: ubuntu-latest
    services:
      redis:
        image: redis:7
        ports:
          - 6379:6379

    steps:
    - name: Checkout Code
      uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 7.0.x

    - name: Wait for Redis to be Ready
      run: |
        for i in {1..10}; do
          if nc -z localhost 6379; then
            echo "Redis is ready!"
            exit 0
          fi
          echo "Waiting for Redis..."
          sleep 2
        done
        echo "Redis failed to start"
        exit 1

    - name: Build and Start API
      run: |
        dotnet build --configuration Release
        dotnet run --configuration Release --no-launch-profile &  # Start API in background
        sleep 5  # Wait for API to start

    - name: Install K6
      run: sudo apt-get install -y k6

    - name: Run Stress Test
      run: k6 run tests/performance/stress-test.js
      env:
        API_URL: http://localhost:5000

