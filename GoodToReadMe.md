# Virtual Power Plant Simulator - Implementation

## Major Changes and Improvements

### 1. Power Distribution Strategy
- Implemented a new `BasicPowerDistributionStrategy` class that implements the `IPowerDistributionStrategy` interface.
- This strategy distributes power proportionally among available batteries based on their capacity.

### 2. Dependency Injection
- Introduced dependency injection using Microsoft.Extensions.DependencyInjection.
- Services are now configured in the `Program.cs` file, improving modularity and testability.

### 3. Battery Pool Enhancements
- Modified `BatteryPool` class to use the `IPowerDistributionStrategy`.
- Implemented `IBatteryPool` interface for better abstraction.

### 4. Power Distribution Algorithm
- The algorithm now handles busy batteries, fully charged, and fully discharged batteries.
- Implements a two-pass approach for initial distribution and adjustment for remaining power.

### 5. Unit Tests
- Added unit tests for `BasicPowerDistributionStrategy` to ensure correct power distribution.
- Implemented tests for handling busy batteries and proportional distribution.

### 6. Code Structure
- Reorganized the codebase to follow better separation of concerns.
- Introduced abstractions (interfaces) for key components to improve extensibility.
