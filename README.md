# Password Manager - C# Desktop App

This project is a password manager my team and I developed for our Data Structures course. Built in C#, the application uses a command-line interface to let users manage stored credentials. It uses AES encryption to protect passwords and SQLite for persistent storage.

## Features
- User login and master password authentication
- AES-256-GCM encryption for stored credentials
- PBKDF2 key derivation using the master password and a random salt
- Unique random nonce for each encryption operation
- SQLite database integration for persistent credential storage
- Password generation with customizable length and character types
- Add, view, update, and delete stored credentials
- Command-line interface for user interaction

## Tools & Technologies
- C# / .NET 8+
- SQLite
- AES-256-GCM
- PBKDF2 (SHA-256)
- Visual Studio

## How to Run It
1. Clone or download the project.
2. Open the `.sln` file in Visual Studio.
3. Check the project's SQLite database configuration and ensure any required dependencies are installed.
4. Build and run the application using .NET 8 or later.
5. On the first launch, create a master password and restart the application.
6. Log in using your master password to generate, store, retrieve, update, or delete credentials.

The application automatically creates a local SQLite database when needed.

## Demo
![Password Manager Application](assets/Screenshot%202026-08-23%20201206.png)

[▶ Watch the full application demo on YouTube](https://www.youtube.com/watch?v=m7onbGe728I)

## My Contributions
This was a team project developed for our Data Structures course. My primary responsibilities included:
- Implementing AES encryption and decryption for stored credentials.
- Developing master password authentication functionality.
- Improving encryption key management by implementing PBKDF2-based key derivation and randomized AES-GCM encryption.

## Why We Built It
We wanted to build something useful and learn more about encryption, databases, and desktop development along the way.
Feel free to use the code as a learning reference, but don’t use real passwords when testing it.
