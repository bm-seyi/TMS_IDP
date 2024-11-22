# Tram Management System - API

## Description

The Tram Management System (TMS) API is designed to handle requests and provide services for the TMS app. It facilitates communication between the app and backend systems, ensuring smooth operation and data management.

The project currently uses default values to seed the database. For scopes, claims, and identity resources, in a production environment, the [ClientCredGen](https://github.com/bm-seyi/ClientCredGen) tool would be used to remove the default data from the database and replace it with the required data. 

As of 22/11/2024, [ClientCredGen](https://github.com/bm-seyi/ClientCredGen) needs to be unarchived and updated to align with the latest changes in this project.

## Features

- Request handling for the TMS app
- Data retrieval and manipulation
- Secure access control

## Endpoint Performance Metrics

| Endpoint          | Response Time (ms) |
|---------------    |--------------------|
| `auth/password`   |1,318               |
| `registration`    | 465                |


## Roadmap

- **Code Coverage:** Eliminated redundant files and directories from code coverage.

- **SignalR:** Integrated SignalR to enable real-time communication with [TMS_APP](https://github.com/bm-seyi/TMS_APP).

## Support

For support, please contact us via email at [seyiadeyemi34@outlook.com](mailto:seyiadeyemi34@outlook.com).

## Contributing

If you're interested in contributing to this project, please reach out via [seyiadeyemi34@outlook.com](mailto:seyiadeyemi34@outlook.com).

## License

This work is licensed under a Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International License.