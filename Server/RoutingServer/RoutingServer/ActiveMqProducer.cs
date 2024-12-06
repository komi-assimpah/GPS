using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.NMS.ActiveMQ;
using Apache.NMS;
using Newtonsoft.Json;

namespace RoutingServer
{
    public class ActiveMqProducer
    {
        private readonly IConnectionFactory connectionFactory;
        private IConnection connection;
        private ISession session;

        public ActiveMqProducer(string brokerUri = "activemq:tcp://localhost:61616")
        {
            connectionFactory = new ConnectionFactory(brokerUri);
        }

        public void Connect()
        {
            try
            {
                connection = connectionFactory.CreateConnection();
                connection.Start();
                session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
                Console.WriteLine("[ActiveMQProducer] Connected to ActiveMQ broker.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ActiveMQProducer] Error connecting to ActiveMQ: " + ex.Message);
                throw;
            }
        }

        public void SendMessage<T>(string queueName, T message)
        {
            try
            {
                if (session == null)
                {
                    Console.WriteLine("[ActiveMQProducer] Session not initialized. Connecting...");
                    Connect();
                }


                // Sérialisation en JSON
                string serializedMessage = JsonConvert.SerializeObject(message);

                IDestination destination = session.GetQueue(queueName);
                using (IMessageProducer producer = session.CreateProducer(destination))
                {
                    ITextMessage textMessage = producer.CreateTextMessage(serializedMessage);
                    producer.Send(textMessage);
                    Console.WriteLine($"[ActiveMQProducer] Message sent to queue '{queueName}': {serializedMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ActiveMQProducer] Error sending message to queue '{queueName}': {ex.Message}");
                throw;
            }
        }


        public void SubscribeToQueue(string queueName, Action<string> onMessageReceived)
        {
            try
            {
                if (session == null)
                {
                    throw new InvalidOperationException("[ActiveMQProducer] Session not initialized. Call Connect() first.");
                }

                IDestination destination = session.GetQueue(queueName);
                using (IMessageConsumer consumer = session.CreateConsumer(destination))
                {
                    consumer.Listener += message =>
                    {
                        if (message is ITextMessage textMessage)
                        {
                            onMessageReceived?.Invoke(textMessage.Text);
                            Console.WriteLine($"[ActiveMQProducer] Message received from queue '{queueName}': {textMessage.Text}");
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ActiveMQProducer] Error subscribing to queue: " + ex.Message);
                throw;
            }
        }

        public void Disconnect()
        {
            try
            {
                session?.Close();
                connection?.Close();
                Console.WriteLine("[ActiveMQProducer] Disconnected from ActiveMQ.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ActiveMQProducer] Error disconnecting from ActiveMQ: " + ex.Message);
                throw;
            }
        }
    }
}