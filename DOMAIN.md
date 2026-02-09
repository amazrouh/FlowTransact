# Domain Design: FlowTransact (MoneyFellows Quest)

## 1. Bounded Contexts
We have identified two primary bounded contexts to ensure independent scalability and data isolation:

### A. Transactions Context (The Source of Truth)
Responsible for managing the lifecycle of a financial intent.

#### Aggregate Root: `Transaction`
**Properties:**
- `Id` (Guid): Unique identifier
- `CustomerId` (Guid): Customer who owns the transaction
- `Status` (TransactionStatus): Current state
- `TotalAmount` (decimal): Calculated sum of all items
- `CreatedAt` (DateTime): Creation timestamp
- `SubmittedAt` (DateTime?): Submission timestamp
- `CompletedAt` (DateTime?): Completion timestamp

**Entities:**
- `TransactionItem`:
  - `Id` (Guid): Unique identifier
  - `ProductId` (Guid): Reference to the product
  - `ProductName` (string): Product name at time of addition
  - `Quantity` (int): Quantity ordered
  - `UnitPrice` (decimal): Price per unit

**State Transitions:**
- `Draft` → `Submitted` → `Completed` OR `Cancelled`
- Transitions are strictly unidirectional

**Invariants:**
- A Transaction cannot be submitted without at least one Item.
- Total Amount must be the sum of all Item prices (Quantity × UnitPrice).
- Transitions are strictly unidirectional: `Draft` → `Submitted` → `Completed`/`Cancelled`.
- A Transaction can only be Cancelled if it is in `Draft` or `Submitted` state. Once `Completed`, it is immutable.

### B. Payments Context (The Executioner)
Responsible for interacting with payment gateways and managing payment states.

#### Aggregate Root: `Payment`
**Properties:**
- `Id` (Guid): Unique identifier
- `TransactionId` (Guid): Reference to the transaction being paid
- `Amount` (decimal): Payment amount
- `Status` (PaymentStatus): Current state
- `CreatedAt` (DateTime): Creation timestamp
- `CompletedAt` (DateTime?): Completion timestamp

**State Transitions:**
- `Pending` → `Completed` OR `Failed`

**Invariants:**
- A Payment cannot be confirmed unless it is in a `Pending` state.
- Idempotency: Each `TransactionId` can only trigger one `Payment` intent.

## 2. Domain Events
To ensure **Eventual Consistency** between contexts, we use these high-value events:

### TransactionSubmitted
- **Publisher:** Transactions Service
- **Consumer:** Payments Service
- **Purpose:** Signals that a transaction is ready for payment processing
- **Payload:** TransactionId, CustomerId, TotalAmount

### PaymentConfirmed
- **Publisher:** Payments Service
- **Consumer:** Transactions Service
- **Purpose:** Confirms successful payment completion
- **Payload:** PaymentId, TransactionId, Amount, ConfirmedAt

### PaymentFailed
- **Publisher:** Payments Service
- **Consumer:** Transactions Service
- **Purpose:** Notifies of payment failure
- **Payload:** PaymentId, TransactionId, Amount, FailedAt, Reason

## 3. Reliability & Consistency (The Senior Edge)
- **Outbox Pattern:** We use the Transactional Outbox pattern to ensure that the database update and the event publication are atomic. This prevents "Ghost Events" or lost messages.
- **Idempotency:** The Payments consumer uses the `TransactionId` as an idempotency key to prevent duplicate payments in case of network retries.
- **State Machine:** The Transaction aggregate acts as a state machine to ignore out-of-order events (e.g., ignoring a 'Fail' event if the transaction was already 'Completed').

## 4. Context Mapping
- **Transactions Context** ↔ **Payments Context**: Asynchronous communication via domain events
- Shared contracts defined in `MoneyFellows.Contracts` library
- No direct database relationships between contexts