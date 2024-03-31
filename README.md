# A proposal for data deduplication on end-to-end encrypted documents


## This project presents a solution for secure storage with the possibility of deduplication

To obtain more efficient solutions for data transfer and storage, data deduplication techniques were designed and implemented. Still, it was proved that those first schemes,
implemented by large companies, such as Dropbox, were
insecure and allowed for unauthorized data access. Since
then, many schemes for secure data deduplication have been
proposed.

From 2010 to the present day,
security properties for data deduplication were tackled mainly
using the following approaches:
- Proof of ownership .
- Message-dependent encryption.
- Traffic Obfuscation
- Deterministic information dispersal 

Alongside the above mentioned approaches, semantic secure
data deduplication schemes were proposed, but were
proven to be unusable from a user-experience perspective.
Most solutions to approach deduplication problem via Proof
of ownership are based on Merkle Hash Tree, which are
a good method to generate and check challenges regarding
data content. Although MHT seem to be a sensible approach
for this solution, MHT are I/O intensive, therefore are not as
efficient as one might wish. In our approach, the server stores a
set of precomputed challenges that are to be used when another
user would claim the ownership of the same document.

Message-dependant encryption started using a simple aproach to encrypt documents using a some information generated from the file content itself. This way, one may assure a
better deduplication factor, since two identical cleartext have
the same cyphertext. This proposals evolved over time by using
key servers and they also could generate a randomized tag, that
is used to identify the document.

To enhance data deduplication efficiency and security, we
propose using a proxy server as an intermediary between the
client and server. This approach expedites Proof of Ownership (PoW) computation, reducing client-side workload and
mitigating security risks from network monitoring attacks. By
acting as a protective barrier, the proxy server enhances overall
performance and security in the deduplication process.

## System diagram

![System_diagram](https://github.com/jipi2/Secure_Data_Deduplication/assets/100122693/53d8c0c0-78f8-4cba-af94-56af5005c828)

## Repo Structure

The repository comprises two folders. The 'web_app' directory contains a project featuring a minimal web interface. This project loads files into memory, which means it's limited by the file size. The second folder, 'desktop_app,' contains a project built as a desktop application, tailored for handling large files. This one is also minimal in nature.

## Web App

### Installing steps

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

You need to execute the following command in the __web_app/mysql_yaml__ folder:
```
docker-compose up -d
```

You can now execute the following command in the __web_app__ folder:
```
docker-compose up -d
```

In the __web_app/FileStorageApp/FileStorageApp/Server/appsettings.json__ file, you will need to specify the container from Azure Blob Storage, the connection string for Azure, the connection string for the SQL Server database, aes key, aes iv, and secret for jwt.

After completing these steps, you can run the application and begin using it.

# Desktop App 

This module is not yet complete.

The proxy module requires Docker to be installed.

In the __PythonProxy__ folder, in __.env__ file, you can modify the link for the main server and other settings.

To install the project, begin by building the images for the proxy:
- navigate to __desktop_app/celery_deploy__ and execute the command:
```
docker build -t celery . 
```
- navigate to __desktop_app/PythonProxy__ and execute the command:
```
docker build -t proxy .
```

You need to execute the following command in the __desktop_app/mysql_yaml__ folder:
```
docker-compose up -d
```

You can now execute the following command in the __desktop_app__ folder:
```
docker-compose up -d
```

In the __desktop_app/FileStorageApp/FileStorageApp/Server/appsettings.json__ file, you will need to specify the container from Azure Blob Storage, the connection string for Azure, the connection string for the SQL Server database, aes key, aes iv, and secret for jwt.

After completing these steps, you can run the application and begin using it. The difference now is that the client app needs to be executed from the DesktopApp, the interface from FileStorageApp can no longer be utilized. 