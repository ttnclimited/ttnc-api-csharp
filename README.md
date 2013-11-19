#TTNC's C# API Client

A simple C# library that handles connection, authentication, requests and parsing of responses to and from TTNC's API. For more information on TTNC's API visit [TTNC's API Pages](http://www.ttnc.co.uk/myttnc/ttnc-api/) or [TTNC's Developer Centre](http://developer.ttnc.co.uk)

A list of function requests available via the API can be found [here](http://developer.ttnc.co.uk/functions/)

## Requirements

- An account with [TTNC](http://www.ttnc.co.uk).
- A VKey (Application) created via [myTTNC](https://www.myttnc.co.uk).

## Usage

The API can be constructed as follows;
```csharp
	using System;
	using TTNCApi;
	
	class Program
	{
		static void Main(string[] args)
		{
			TTNCApi api = new TTNCApi("<username>", "<password>", "<vkey>");
			TTNCRequest request = api.NewRequest("NoveroNumbers", "ListNumbers", "Request1");
			api.MakeRequests();
			TTNCParser dic = request.GetResponse();
         }
    }
```

Requests can then be 'spooled' in the object until the *MakeRequests* method is called. While not required, each request should be given an ID which can be used to retrieve the response later on;

```csharp
	TTNCRequest request = api.NewRequest("NoveroNumbers", "ListNumbers", "Request1");
```

### Basic Usage
```csharp
	using System;
	using TTNCApi;
	
	class Program
	{
		static void Main(string[] args)
		{
			TTNCApi api = new TTNCApi("<username>", "<password>", "<vkey>");
			TTNCRequest request = api.NewRequest("NoveroNumbers", "ListNumbers", "Request1");
			api.MakeRequests();
			TTNCParser dic = request.GetResponse();
         }
    }	
```

In order to send data in a request - the *setData* method can  be called on the *Request* object;

```csharp
	using System;
	using TTNCApi;
	
	class Program
	{
		static void Main(string[] args)
		{
			TTNCApi api = new TTNCApi("<username>", "<password>", "<vkey>");
			TTNCRequest request = api.NewRequest("NoveroNumbers", "ListNumbers", "Request1");
			request.setData("Number", "02031511000");
			request.setData("Destination", "07512312312");
			api.MakeRequests();
			TTNCParser dic = request.GetResponse();
         }
    }
```

### Parsing Responses

The response can be retrieved from the request object after *MakeRequests* has been called.
```csharp
	TTNCParser dic = request.GetResponse();
```

Alternatively, you can retrieve the response from the response from the API based on the ID passed to the request;

```csharp
	TTNCParser dic = api.GetResponseFromId('Request1');
```

### Advanced Usage

The client deals automatically with the *Auth* requests for you, however, in order to perform some more advanced actions on the API (such as ordering numbers via the *AddToBasket* request) you may need to save the session state between script executions. In order to do this it's necessary to access the *SessionRequest* Response and retrieve the returned SessionId. This can then be stored in your own code for use on the next request;

```csharp
	using System;
	using TTNCApi;
	
	class Program
	{
		static void Main(string[] args)
		{
			TTNCApi api = new TTNCApi("<username>", "<password>", "<vkey>");
			TTNCRequest request = api.NewRequest("Order", "AddToBasket", "Request1");
			request.setData("number", "02031231231");
			request.setData("type", "number");
			api.MakeRequests();
			TTNCParser dic = api.GetResponseFromId('SessionRequest');
         }
    }
	
	# Store dic['SessionId'] in your own code.
```

Then on repeat requests, to retrieve the same basket you can construct the object without authentication and then parse in the SessionId to use on Requests;

```csharp
	using System;
	using TTNCApi;
	
	class Program
	{
		static void Main(string[] args)
		{
			TTNCApi api = new TTNCApi();
			api.usesession(<SessionId>); # From the previous request, stored in your own code
			TTNCRequest request = api.NewRequest("Order", "ViewBasket", "Request1");
			api.MakeRequests();
			TTNCParser dic = request.GetResponse();
         }
    }
    # dic now contains a representation of your basket.
```

## Getting Support

If you have any questions or support queries then first please read the [Developers Site](http://developer.ttnc.co.uk) and then email support@ttnc.co.uk.

