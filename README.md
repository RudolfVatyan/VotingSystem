# Blockchain-Based Voting System

This project implements a decentralized voting platform that combines traditional web technologies with blockchain to ensure secure, transparent, and tamper-proof elections. The backend is built with ASP.NET Core Web API, while the frontend is a React single-page application. Votes are recorded and verified on the Ethereum Sepolia testnet through smart contracts, providing an immutable ledger of all voting activity.

---

## Features

- **Secure User Authentication:** Uses JWT tokens to authenticate users and secure API endpoints.
- **Blockchain Voting:** Votes are cast through Ethereum smart contracts to guarantee data integrity.
- **Admin Controls:** Admins can add candidates, start/end voting periods, and view live vote tallies.
- **Real-time Vote Results:** The frontend fetches results directly from the blockchain to ensure accuracy.
- **Modular Architecture:** Backend and frontend are separated for easy maintenance and scalability.
- **Configurable via `appsettings.json`:** Connect your own Ethereum node (via Infura), contract address, and wallet private key.

---

## Technologies Used

| Layer       | Technology                                  |
|-------------|---------------------------------------------|
| Backend     | ASP.NET Core Web API, C#                     |
| Blockchain  | Solidity smart contracts, Ethereum Sepolia testnet |
| Blockchain Interaction | Nethereum (.NET Ethereum library)         |
| Authentication | JWT (JSON Web Tokens)                        |
| Frontend    | React, JavaScript, Axios, CSS Modules       |
| Infrastructure | Infura (Ethereum node provider)              |

---

## Project Structure

VotingSystem/
├── Controllers/ # API endpoints (VotingController, UserController, etc.)
├── Services/ # Business logic & blockchain interaction services
├── Models/ # Data models (User, Candidate, Vote)
├── Migrations/ # EF Core database migrations (if any)
├── voting-frontend/ # React frontend app source code
├── appsettings.json # Configuration file (keys, contract address)
├── Program.cs # Backend entry point
├── Startup.cs # Dependency injection, middleware config
└── README.md # Project documentation (this file)


---

## Setup & Installation

### Prerequisites

- [.NET 6 SDK or newer](https://dotnet.microsoft.com/download)
- [Node.js & npm](https://nodejs.org/en/download/)
- Infura account to get an Ethereum project ID: [https://infura.io/](https://infura.io/)
- Ethereum wallet private key (testnet)
- Deployed smart contract address on Sepolia testnet

---

### 1. Clone the repository

```bash
git clone https://github.com/RudolfVatyan/VotingSystem.git
cd VotingSystem
2. Backend Configuration
Edit appsettings.json to include:

{
  "InfuraProjectId": "your-infura-project-id",
  "PrivateKey": "your-ethereum-wallet-private-key",
  "ContractAddress": "your-smart-contract-address"
}
3. Run Backend API
Restore dependencies and start the server:

dotnet restore
dotnet run
By default, the API will run at https://localhost:5001 and http://localhost:5000.

4. Frontend Setup & Run
Navigate to the React frontend folder:


cd voting-frontend
npm install
npm start
This will start the React development server at http://localhost:3000.

Make sure both backend and frontend are running simultaneously.

How It Works
Backend
Provides REST API endpoints for authentication, voting, and admin actions.

Connects to the Ethereum blockchain through Nethereum and Infura.

Interacts with the smart contract for vote submission, candidate management, and retrieving results.

Handles JWT token generation and validation for secure access.

Frontend
React app that handles user login and registration.

Displays candidates and allows users to cast votes.

Shows real-time vote counts pulled from the blockchain.

Admin interface for adding candidates and controlling voting periods.

Blockchain
Solidity smart contract deployed on the Sepolia testnet.

Prevents double voting by tracking voters’ addresses.

Stores candidate information and vote counts securely.

Voting period controlled via start and end timestamps.

Smart Contract Details
The contract includes the following main functions:

addCandidate(string candidateName): Admin adds new candidates.

vote(string candidateName): Users cast their vote for a candidate.

getVotes(string candidateName): Returns vote count for a candidate.

hasVoted(address voter): Checks if an address has already voted.

setVotingPeriod(uint start, uint end): Admin sets the voting start and end time.

Usage Notes
Use a testnet wallet with some Sepolia ETH for transaction fees.

JWT tokens are required to authenticate API calls.

Votes are processed on-chain; expect minor delays due to blockchain confirmation times.

Admin actions require elevated permissions (backend currently allows based on user role).

Future Improvements
Add frontend authentication flow with token refresh.

Implement user roles and permission management on backend.

Enhance UI with better responsiveness and accessibility.

Integrate notifications for vote success/failure.

Add deployment scripts for easier hosting.

Contribution
Feel free to open issues or submit pull requests. Suggestions and bug reports are welcome!

Author
Rudolf Vatyan
GitHub Profile
