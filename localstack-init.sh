#!/bin/bash

echo "Initializing LocalStack resources for Production Service..."

# Create SNS Topic
awslocal sns create-topic --name sns-production-events

# Create SQS Queue
awslocal sqs create-queue --queue-name sqs-production-orders

# Subscribe queue to topic (fan-out pattern)
TOPIC_ARN="arn:aws:sns:us-east-1:000000000000:sns-production-events"
QUEUE_ARN="arn:aws:sqs:us-east-1:000000000000:sqs-production-orders"

awslocal sns subscribe \
  --topic-arn $TOPIC_ARN \
  --protocol sqs \
  --notification-endpoint $QUEUE_ARN

echo "LocalStack initialization complete!"
