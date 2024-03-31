# Web App

## Installing steps

The proxy module requires Docker to be installed.

In the __PythonProxy__ folder, in __.env__ file, you can modify the link for the main server and other settings.

To install the project, begin by building the images for the proxy:
- navigate to __web_app/celery_deploy__ and execute the command:
```
docker build -t celery . 
```
- navigate to __web_app/PythonProxy__ and execute the command:
```
docker build -t proxy .
```

You need to execute the following command in the __mysql_yaml__ folder:
```
docker-compose up -d
```

You can now execute the following command in the __web_app__ folder:
```
docker-compose up -d
```

In the __web_app/FileStorageApp/FileStorageApp/Server/appsettings.json__ file, you will need to specify the container from Azure Blob Storage, the connection string for Azure, the connection string for the SQL Server database, aes key, aes iv, and secret for jwt.

After completing these steps, you can run the application and begin using it.