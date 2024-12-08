# Let's Go Biking 🚴‍♂️

## 📝 Description  
**Let's Go Biking** is a project designed to create a web application that allows users to **plan their routes**, **track their itineraries in real time**, and **visualize each step on a dynamic map**. It also provides a heavy (desktop) client for the routing server.

This application is built on **C# self-hosted servers**, utilizing a **SOAP server** for proxy and caching and a **REST server** for routing.

It leverages both the **JCDecaux API** and the **OpenRouteService API** to deliver itinerary planning services. Users can request and display these itineraries through the client application.

---

## 🚀 Demo

### 🌐 Light Client  
**🔵 = JCDecaux bike itinerary**  
**🔴 = Walking itinerary**

**🎥 Demonstration Video**  
You can view a demonstration of the application here:  
*Demo Video Link* *(Replace with an actual link)*

### 💻 Heavy Client

---

## ✨ Features

- **📍 Dynamic Itinerary Calculation**: Generate routes between a start position and a destination.  
- **🗺️ Route Visualization**: Display generated itineraries on an interactive map (Leaflet.js).  
- **🚶‍♂️/🚴‍♂️ Animations**: Visually follow a marker representing a cyclist or a pedestrian along the route.  
- **⏱️ Remaining Time**: Calculate and show the estimated time to complete the itinerary.  
- **🎛️ UI Reactivity**: Hide the sidebar during itinerary execution for a more immersive map experience.  
- **🌐 Self-Hosted SOAP Server**: A C# SOAP server for proxy and cache functionalities.  
- **🧩 Custom Web Components**: Modular components for elements like search bars and itinerary suggestions.

---

## 🛠️ Installation

### 📦 Prerequisites

- **.NET Framework 4.8**  
- **ActiveMQ**  
- **Node.js** (from [nodejs.org](https://nodejs.org/))  
- **http-server** (for serving static files)

### 💻 Launching

#### Step 1: Install Dependencies

Install `http-server` globally:
```
npm install -g http-server
```

Ensure that ActiveMQ is running:
```
activemq start
```

#### Step 2: Start the Servers
Navigate to the directory containing the servers:

```
..\GPS\Server\RoutingServer\RoutingServer\bin\Debug
```


Run the executables `ProxyCache` and `RoutingServer`. If they are not available, rebuild the project in Visual Studio.


#### Step 3: Launch the Light Client

Navigate to the light client directory:
```
..\GPS\Client
```

Execute the command:
```
http-server -p 5501
```

Open your browser and access the application at:
```
http://localhost:5501
```



---

## 👥 Authors

- [**Jean Paul ASSIMPAH**](https://github.com/komi-assimpah)  
- [**Sagesse ADABADJI**](https://github.com/Sagesse554)
