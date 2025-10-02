# Industry-Standard Multiplayer Game Server Architecture

## 🏗️ Complete From-Scratch Build Plan

**Goal:** Production-ready, scalable, real-time multiplayer game server for 2-8 player party game

---

## Architecture Overview

### High-Level Design

```
┌─────────────────────────────────────────────────────────┐
│                    CLIENTS (Unity)                      │
│  Desktop (Host) + Mobile Players (2-8 total)           │
└─────────────────┬───────────────────────────────────────┘
                  │ WebSocket (wss://)
                  ↓
┌─────────────────────────────────────────────────────────┐
│              LOAD BALANCER (NGINX/HAProxy)              │
│         SSL Termination + WebSocket Upgrade             │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ↓
┌─────────────────────────────────────────────────────────┐
│           GAME SERVER CLUSTER (Node.js)                 │
│                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Server 1   │  │   Server 2   │  │   Server N   │  │
│  │  (Stateless) │  │  (Stateless) │  │  (Stateless) │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
│         │                  │                  │          │
│         └──────────────────┴──────────────────┘          │
│                            │                             │
└────────────────────────────┼─────────────────────────────┘
                             │
                             ↓
┌─────────────────────────────────────────────────────────┐
│            SHARED STATE LAYER (Redis Cluster)           │
│                                                          │
│  • Room State (Active Games)                            │
│  • Player Sessions (Who's in which room)                │
│  • Vote Tracking (Real-time aggregation)                │
│  • Pub/Sub (Inter-server communication)                 │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ↓
┌─────────────────────────────────────────────────────────┐
│         PERSISTENT DATABASE (PostgreSQL)                │
│                                                          │
│  • User Accounts (Optional)                             │
│  • Game History (Completed games)                       │
│  • Statistics & Leaderboards                            │
│  • Question Pool                                        │
└─────────────────────────────────────────────────────────┘
                  │
                  ↓
┌─────────────────────────────────────────────────────────┐
│         MONITORING & LOGGING (ELK/Prometheus)           │
│                                                          │
│  • Application Metrics                                  │
│  • Error Tracking (Sentry)                              │
│  • Real-time Dashboards                                 │
└─────────────────────────────────────────────────────────┘
```

---

## Technology Stack (Industry Standard)

### Backend Server
**Node.js + TypeScript** ✅ Recommended

**Why:**
- Excellent WebSocket support (ws library)
- High concurrency (event-loop model)
- Real-time performance
- Large ecosystem
- TypeScript for type safety

**Alternatives:**
- **Go:** Even better performance, harder to find developers
- **Elixir/Phoenix:** Built for real-time, great for multiplayer, smaller ecosystem
- **C# (.NET):** Good if Unity backend, SignalR for WebSockets

### WebSocket Library
**ws (Node.js)** - Most mature, battle-tested

```bash
npm install ws
npm install @types/ws --save-dev
```

### State Management
**Redis Cluster** ✅ Critical

**Why:**
- Sub-millisecond latency
- Pub/Sub for real-time events
- Automatic failover
- Data structures (Hash, List, Set)
- Atomic operations for vote counting

**Configuration:**
- 3+ node cluster
- AOF + RDB persistence
- Keyspace notifications enabled

### Persistent Database
**PostgreSQL 15+** ✅ Recommended

**Why:**
- JSONB for flexible game state
- Strong ACID guarantees
- Excellent performance
- Rich indexing
- PostGIS if adding location features

**Schema:**
```sql
-- Users (optional, if accounts needed)
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Questions Pool
CREATE TABLE questions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    question_text TEXT NOT NULL,
    correct_answer VARCHAR(255) NOT NULL,
    robot_answer VARCHAR(255) NOT NULL,
    type VARCHAR(20) NOT NULL, -- 'text' or 'picture'
    image_url TEXT,
    round_number INTEGER,
    difficulty VARCHAR(20),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Game History
CREATE TABLE games (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    room_code VARCHAR(4) NOT NULL,
    game_mode VARCHAR(20) NOT NULL, -- '8-round' or '12-round'
    started_at TIMESTAMP NOT NULL,
    completed_at TIMESTAMP,
    winner_id UUID REFERENCES users(id),
    game_state JSONB, -- Full game state snapshot
    INDEX idx_room_code (room_code),
    INDEX idx_started_at (started_at)
);

-- Player Game Stats
CREATE TABLE player_game_stats (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    game_id UUID REFERENCES games(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id),
    player_name VARCHAR(50) NOT NULL,
    final_score INTEGER NOT NULL,
    placement INTEGER NOT NULL,
    rounds_won INTEGER DEFAULT 0,
    correct_answers INTEGER DEFAULT 0,
    robots_identified INTEGER DEFAULT 0,
    times_fooled INTEGER DEFAULT 0,
    INDEX idx_game_id (game_id),
    INDEX idx_user_id (user_id)
);

-- Leaderboard Cache (for fast queries)
CREATE MATERIALIZED VIEW leaderboard AS
SELECT
    player_name,
    COUNT(*) as games_played,
    SUM(CASE WHEN placement = 1 THEN 1 ELSE 0 END) as wins,
    AVG(final_score) as avg_score,
    SUM(correct_answers) as total_correct,
    SUM(robots_identified) as total_robots_identified
FROM player_game_stats
GROUP BY player_name
ORDER BY wins DESC, avg_score DESC;
```

### Message Queue (Optional, for scale)
**RabbitMQ** or **Apache Kafka**

**When needed:**
- 1000+ concurrent games
- Cross-region deployment
- Event sourcing architecture

### Load Balancer
**NGINX** ✅ Industry standard

**Configuration:**
```nginx
upstream game_servers {
    ip_hash; # Sticky sessions for WebSocket
    server gameserver1:3000;
    server gameserver2:3000;
    server gameserver3:3000;
}

server {
    listen 443 ssl http2;
    server_name game.yourdomain.com;

    ssl_certificate /etc/ssl/certs/fullchain.pem;
    ssl_certificate_key /etc/ssl/private/privkey.pem;

    location /ws {
        proxy_pass http://game_servers;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_read_timeout 86400; # 24 hours
    }
}
```

### Monitoring & Logging

**Application Metrics:** Prometheus + Grafana
- Active connections
- Rooms active/completed
- Average game duration
- Vote latency
- Error rates

**Logging:** Winston (Node.js) → ELK Stack
- Centralized log aggregation
- Full-text search
- Real-time alerts

**Error Tracking:** Sentry
- Exception capture
- Stack traces
- User context
- Performance monitoring

**Health Checks:** Custom endpoint
```typescript
app.get('/health', (req, res) => {
    res.json({
        status: 'healthy',
        uptime: process.uptime(),
        connections: wss.clients.size,
        rooms: roomManager.getActiveRoomCount(),
        redis: await redis.ping(),
        db: await db.query('SELECT 1')
    });
});
```

---

## Server Architecture Design

### Layered Architecture

```
┌─────────────────────────────────────────┐
│         API/WebSocket Layer             │
│  • Connection handling                  │
│  • Message routing                      │
│  • Authentication                       │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         Service Layer                   │
│  • RoomService                          │
│  • GameService                          │
│  • VoteService                          │
│  • ScoringService                       │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         Domain Layer                    │
│  • Room (entity)                        │
│  • Player (entity)                      │
│  • Game (entity)                        │
│  • Vote (value object)                  │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         Data Layer                      │
│  • RedisRepository                      │
│  • PostgresRepository                   │
│  • CacheManager                         │
└─────────────────────────────────────────┘
```

### Core Services

#### 1. RoomService
```typescript
class RoomService {
    async createRoom(hostPlayerId: string): Promise<Room>
    async joinRoom(roomCode: string, playerId: string): Promise<Room>
    async leaveRoom(roomCode: string, playerId: string): Promise<void>
    async startGame(roomCode: string): Promise<void>
    async getRoomState(roomCode: string): Promise<RoomState>
    async closeRoom(roomCode: string): Promise<void>
}
```

#### 2. GameService
```typescript
class GameService {
    async startRound(roomId: string, roundNumber: number): Promise<Question>
    async submitAnswer(roomId: string, playerId: string, answer: string): Promise<void>
    async checkAllAnswersSubmitted(roomId: string): Promise<boolean>
    async transitionPhase(roomId: string, phase: GamePhase): Promise<void>
    async getGameState(roomId: string): Promise<GameState>
}
```

#### 3. VoteService
```typescript
class VoteService {
    async submitEliminationVote(roomId: string, playerId: string, answer: string): Promise<void>
    async submitFinalVote(roomId: string, playerId: string, answer: string): Promise<void>
    async aggregateEliminationVotes(roomId: string): Promise<VoteResults>
    async aggregateFinalVotes(roomId: string): Promise<VoteResults>
    async checkAllVoted(roomId: string, voteType: VoteType): Promise<boolean>
}
```

#### 4. ScoringService
```typescript
class ScoringService {
    calculateRoundScores(
        votes: VoteResults,
        correctAnswer: string,
        robotAnswer: string,
        gameMode: GameMode
    ): Map<string, RoundScore>

    updatePlayerScores(roomId: string, roundScores: Map<string, RoundScore>): Promise<void>
    getFinalStandings(roomId: string): Promise<PlayerScore[]>
    determineWinner(scores: PlayerScore[]): string
}
```

---

## Data Models

### Redis Data Structures

```typescript
// Room State (Hash)
room:{roomCode} = {
    id: string,
    code: string,
    hostPlayerId: string,
    players: Player[], // JSON stringified
    gameMode: '8-round' | '12-round',
    currentPhase: 'lobby' | 'question' | 'elimination' | 'voting' | 'results',
    currentRound: number,
    currentQuestion: Question, // JSON stringified
    createdAt: timestamp,
    expiresAt: timestamp
}

// Player Session (Hash)
player:{playerId} = {
    id: string,
    name: string,
    icon: string,
    roomCode: string,
    connectionId: string,
    isHost: boolean,
    score: number,
    lastActivity: timestamp
}

// Active Game State (Hash)
game:{roomCode} = {
    answers: Answer[], // JSON stringified
    eliminationVotes: Map<playerId, answer>, // JSON stringified
    finalVotes: Map<playerId, answer>, // JSON stringified
    eliminatedAnswers: string[],
    roundScores: RoundScore[], // JSON stringified
    gameHistory: Round[] // JSON stringified
}

// Vote Tracking (Hash) - Atomic operations
votes:elimination:{roomCode} = {
    [playerId]: answer,
    [playerId]: answer,
    ...
}

votes:final:{roomCode} = {
    [playerId]: answer,
    [playerId]: answer,
    ...
}

// Pub/Sub Channels
channel:room:{roomCode} - All room events
channel:global - Server-wide events
```

### TypeScript Interfaces

```typescript
interface Room {
    id: string;
    code: string; // 4-char code
    hostPlayerId: string;
    players: Player[];
    gameMode: GameMode;
    currentPhase: GamePhase;
    currentRound: number;
    currentQuestion?: Question;
    createdAt: Date;
    expiresAt: Date;
}

interface Player {
    id: string;
    name: string;
    icon: string;
    roomCode?: string;
    connectionId: string;
    isHost: boolean;
    score: number;
    isConnected: boolean;
    lastActivity: Date;
}

interface Question {
    id: string;
    text: string;
    correctAnswer: string;
    robotAnswer: string;
    type: 'text' | 'picture';
    imageUrl?: string;
    round: number;
}

interface Answer {
    text: string;
    type: 'player' | 'correct' | 'robot';
    playerName: string;
    timestamp: Date;
}

interface VoteResults {
    votes: Map<string, string>; // playerId -> answer
    voteCounts: Map<string, number>; // answer -> count
    eliminatedAnswer?: string;
    tieOccurred: boolean;
}

interface RoundScore {
    playerName: string;
    correctAnswerPoints: number;
    robotIdentifiedPoints: number;
    votesReceivedPoints: number;
    fooledPenalty: number;
    total: number;
}

interface GameState {
    roomCode: string;
    phase: GamePhase;
    round: number;
    question?: Question;
    answers: Answer[];
    timeRemaining: number;
    players: PlayerStatus[];
}

type GamePhase = 'lobby' | 'question' | 'elimination' | 'voting' | 'results' | 'final';
type GameMode = '8-round' | '12-round';
```

---

## Message Protocol

### Client → Server Messages

```typescript
// Room Management
{
    action: 'create-room',
    playerName: string,
    playerIcon: string,
    gameMode?: '8-round' | '12-round'
}

{
    action: 'join-room',
    roomCode: string,
    playerName: string,
    playerIcon: string
}

{
    action: 'start-game',
    roomCode: string
}

// Gameplay
{
    action: 'submit-answer',
    roomCode: string,
    playerName: string,
    answer: string,
    timestamp: number
}

{
    action: 'submit-elimination-vote',
    roomCode: string,
    playerName: string,
    vote: string
}

{
    action: 'submit-final-vote',
    roomCode: string,
    playerName: string,
    vote: string
}

// Host Actions
{
    action: 'elimination-time-expired',
    roomCode: string
}

{
    action: 'final-voting-time-expired',
    roomCode: string
}

{
    action: 'request-next-round',
    roomCode: string
}
```

### Server → Client Messages

```typescript
// Connection Events
{
    type: 'connected',
    data: {
        playerId: string,
        sessionToken: string
    }
}

{
    type: 'error',
    data: {
        code: string,
        message: string
    }
}

// Room Events
{
    type: 'room-joined',
    roomCode: string,
    data: {
        roomCode: string,
        players: Player[],
        isHost: boolean
    }
}

{
    type: 'player-joined',
    roomCode: string,
    data: {
        player: Player
    }
}

{
    type: 'player-left',
    roomCode: string,
    data: {
        playerId: string,
        playerName: string
    }
}

{
    type: 'players-update',
    roomCode: string,
    data: {
        players: Player[]
    }
}

// Game Events
{
    type: 'round-started',
    roomCode: string,
    data: {
        roundNumber: number,
        question: Question,
        timeLimit: number
    }
}

{
    type: 'player-answered',
    roomCode: string,
    data: {
        playerName: string,
        playerIcon: string
    }
}

{
    type: 'all-answers-submitted',
    roomCode: string,
    data: {
        answers: Answer[]
    }
}

{
    type: 'elimination-vote-cast',
    roomCode: string,
    data: {
        playerName: string,
        voteCount: number,
        totalVotes: number
    }
}

{
    type: 'elimination-complete',
    roomCode: string,
    data: {
        eliminatedAnswer: string | null,
        tieOccurred: boolean,
        voteCounts: Map<string, number>
    }
}

{
    type: 'all-votes-submitted',
    roomCode: string,
    data: {
        correctAnswer: string,
        robotAnswer: string,
        voteCounts: Map<string, number>
    }
}

{
    type: 'final-round-scores',
    roomCode: string,
    data: {
        roundNumber: number,
        scores: RoundScore[],
        standings: PlayerScore[]
    }
}

{
    type: 'game-complete',
    roomCode: string,
    data: {
        winner: string,
        finalStandings: PlayerScore[]
    }
}
```

---

## State Management Strategy

### Room Lifecycle

```typescript
class RoomManager {
    // Create room
    async createRoom(hostPlayer: Player, gameMode: GameMode): Promise<Room> {
        const code = this.generateRoomCode(); // 4-char unique
        const room: Room = {
            id: uuid(),
            code,
            hostPlayerId: hostPlayer.id,
            players: [hostPlayer],
            gameMode,
            currentPhase: 'lobby',
            currentRound: 0,
            createdAt: new Date(),
            expiresAt: new Date(Date.now() + 4 * 60 * 60 * 1000) // 4 hours
        };

        // Store in Redis
        await redis.hset(`room:${code}`, room);
        await redis.expire(`room:${code}`, 14400); // 4 hours TTL

        // Add to active rooms index
        await redis.sadd('rooms:active', code);

        return room;
    }

    // Join room
    async joinRoom(roomCode: string, player: Player): Promise<Room> {
        const room = await this.getRoom(roomCode);

        if (!room) throw new Error('Room not found');
        if (room.players.length >= 8) throw new Error('Room full');
        if (room.currentPhase !== 'lobby') throw new Error('Game already started');

        room.players.push(player);
        await redis.hset(`room:${roomCode}`, 'players', JSON.stringify(room.players));

        // Broadcast to room
        await this.broadcastToRoom(roomCode, {
            type: 'player-joined',
            roomCode,
            data: { player }
        });

        return room;
    }

    // Cleanup expired rooms
    async cleanupExpiredRooms(): Promise<void> {
        const now = Date.now();
        const activeCodes = await redis.smembers('rooms:active');

        for (const code of activeCodes) {
            const room = await this.getRoom(code);
            if (!room || room.expiresAt.getTime() < now) {
                await this.closeRoom(code);
            }
        }
    }
}
```

### Game Flow State Machine

```typescript
class GameStateMachine {
    async transition(roomCode: string, toPhase: GamePhase): Promise<void> {
        const room = await roomManager.getRoom(roomCode);
        const fromPhase = room.currentPhase;

        // Validate transition
        if (!this.isValidTransition(fromPhase, toPhase)) {
            throw new Error(`Invalid transition: ${fromPhase} -> ${toPhase}`);
        }

        // Execute transition
        await this.executeTransition(room, toPhase);

        // Update room state
        room.currentPhase = toPhase;
        await redis.hset(`room:${roomCode}`, 'currentPhase', toPhase);

        // Broadcast state change
        await roomManager.broadcastToRoom(roomCode, {
            type: 'phase-transition',
            roomCode,
            data: { phase: toPhase }
        });
    }

    private isValidTransition(from: GamePhase, to: GamePhase): boolean {
        const validTransitions: Record<GamePhase, GamePhase[]> = {
            'lobby': ['question'],
            'question': ['elimination'],
            'elimination': ['voting'],
            'voting': ['results'],
            'results': ['question', 'final'],
            'final': []
        };

        return validTransitions[from]?.includes(to) ?? false;
    }

    private async executeTransition(room: Room, phase: GamePhase): Promise<void> {
        switch (phase) {
            case 'question':
                await this.startQuestionPhase(room);
                break;
            case 'elimination':
                await this.startEliminationPhase(room);
                break;
            case 'voting':
                await this.startVotingPhase(room);
                break;
            case 'results':
                await this.startResultsPhase(room);
                break;
            case 'final':
                await this.endGame(room);
                break;
        }
    }
}
```

### Vote Aggregation (Atomic)

```typescript
class VoteAggregator {
    async submitEliminationVote(
        roomCode: string,
        playerId: string,
        answer: string
    ): Promise<void> {
        // Atomic operation - prevents race conditions
        const key = `votes:elimination:${roomCode}`;
        await redis.hset(key, playerId, answer);

        // Check if all voted
        const room = await roomManager.getRoom(roomCode);
        const voteCount = await redis.hlen(key);

        if (voteCount === room.players.length) {
            // All voted - aggregate results
            await this.processEliminationVotes(roomCode);
        }
    }

    private async processEliminationVotes(roomCode: string): Promise<void> {
        const votes = await redis.hgetall(`votes:elimination:${roomCode}`);
        const voteCounts = new Map<string, number>();

        // Count votes
        Object.values(votes).forEach(answer => {
            voteCounts.set(answer, (voteCounts.get(answer) || 0) + 1);
        });

        // Find most voted
        let maxVotes = 0;
        let topAnswers: string[] = [];

        voteCounts.forEach((count, answer) => {
            if (count > maxVotes) {
                maxVotes = count;
                topAnswers = [answer];
            } else if (count === maxVotes) {
                topAnswers.push(answer);
            }
        });

        const result: VoteResults = {
            votes: new Map(Object.entries(votes)),
            voteCounts,
            eliminatedAnswer: topAnswers.length === 1 ? topAnswers[0] : undefined,
            tieOccurred: topAnswers.length > 1
        };

        // Broadcast results
        await roomManager.broadcastToRoom(roomCode, {
            type: 'elimination-complete',
            roomCode,
            data: result
        });

        // Clear votes for next phase
        await redis.del(`votes:elimination:${roomCode}`);
    }
}
```

---

## Scaling Strategy

### Horizontal Scaling

**Stateless Servers:**
- Each server node handles WebSocket connections
- No session affinity required (Redis stores state)
- Can spin up/down based on load

**Redis Cluster:**
- Sharded by room code
- 16384 hash slots
- 3+ master nodes, 3+ replicas
- Automatic failover

**Database Scaling:**
- Read replicas for analytics queries
- Connection pooling (pgBouncer)
- Partitioning by date for game history

### Load Distribution

```typescript
// Consistent hashing for room assignment
class RoomLoadBalancer {
    private servers: string[] = ['server1', 'server2', 'server3'];

    getServerForRoom(roomCode: string): string {
        const hash = this.hashCode(roomCode);
        const index = hash % this.servers.length;
        return this.servers[index];
    }

    private hashCode(str: string): number {
        let hash = 0;
        for (let i = 0; i < str.length; i++) {
            hash = ((hash << 5) - hash) + str.charCodeAt(i);
            hash |= 0;
        }
        return Math.abs(hash);
    }
}
```

### Performance Targets

| Metric | Target | Notes |
|--------|--------|-------|
| WebSocket latency | <50ms | 95th percentile |
| Message throughput | 10,000/sec | Per server node |
| Concurrent games | 1,000+ | Per server node |
| Room creation time | <100ms | End-to-end |
| Vote aggregation | <10ms | Redis atomic ops |
| Database write | <50ms | Async, non-blocking |
| Max players per server | 8,000 | 1,000 rooms × 8 players |

---

## Security Considerations

### WebSocket Security

```typescript
// JWT-based authentication
interface SessionToken {
    playerId: string;
    roomCode?: string;
    isHost: boolean;
    exp: number;
}

function verifyToken(token: string): SessionToken {
    return jwt.verify(token, process.env.JWT_SECRET!) as SessionToken;
}

// Message validation
function validateMessage(message: any): boolean {
    // Schema validation
    if (!message.action || typeof message.action !== 'string') {
        return false;
    }

    // Rate limiting check
    if (isRateLimited(message.playerId)) {
        return false;
    }

    // Sanitize inputs
    if (message.answer) {
        message.answer = sanitize(message.answer);
    }

    return true;
}
```

### Rate Limiting

```typescript
class RateLimiter {
    private limits = new Map<string, number[]>();

    isAllowed(playerId: string, maxPerMinute: number = 60): boolean {
        const now = Date.now();
        const timestamps = this.limits.get(playerId) || [];

        // Remove old timestamps
        const recent = timestamps.filter(ts => now - ts < 60000);

        if (recent.length >= maxPerMinute) {
            return false;
        }

        recent.push(now);
        this.limits.set(playerId, recent);
        return true;
    }
}
```

### Input Sanitization

```typescript
function sanitizeAnswer(answer: string): string {
    // Max length
    answer = answer.substring(0, 100);

    // Remove dangerous characters
    answer = answer.replace(/[<>\"\']/g, '');

    // Trim whitespace
    answer = answer.trim();

    return answer;
}

function containsProfanity(text: string): boolean {
    // Use library like 'bad-words'
    const filter = new Filter();
    return filter.isProfane(text);
}
```

### DDoS Protection

- CloudFlare or AWS Shield
- Connection rate limiting per IP
- WebSocket frame size limits
- Automatic ban for abuse patterns

---

## Deployment Architecture

### Infrastructure as Code (Terraform)

```hcl
# AWS ECS Cluster for game servers
resource "aws_ecs_cluster" "game_servers" {
  name = "robots-game-cluster"
}

# Auto-scaling based on CPU/connections
resource "aws_appautoscaling_target" "game_servers" {
  max_capacity       = 10
  min_capacity       = 2
  resource_id        = "service/${aws_ecs_cluster.game_servers.name}/${aws_ecs_service.game_server.name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

# Redis Cluster (ElastiCache)
resource "aws_elasticache_replication_group" "redis" {
  replication_group_id          = "robots-game-redis"
  replication_group_description = "Redis cluster for game state"
  engine                        = "redis"
  engine_version               = "7.0"
  node_type                    = "cache.r6g.large"
  number_cache_clusters        = 3
  automatic_failover_enabled   = true
  multi_az_enabled            = true
}

# PostgreSQL (RDS)
resource "aws_db_instance" "postgres" {
  identifier           = "robots-game-db"
  engine              = "postgres"
  engine_version      = "15.3"
  instance_class      = "db.t3.medium"
  allocated_storage   = 100
  storage_encrypted   = true
  multi_az            = true
  backup_retention_period = 7
}

# Load Balancer (ALB with WebSocket support)
resource "aws_lb" "game_lb" {
  name               = "robots-game-lb"
  load_balancer_type = "application"
  subnets           = aws_subnet.public[*].id
  security_groups   = [aws_security_group.lb.id]
}
```

### CI/CD Pipeline (GitHub Actions)

```yaml
name: Deploy Game Server

on:
  push:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      - run: npm ci
      - run: npm test
      - run: npm run lint

  build:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: docker/build-push-action@v4
        with:
          push: true
          tags: ghcr.io/yourorg/game-server:${{ github.sha }}

  deploy:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to ECS
        run: |
          aws ecs update-service \
            --cluster robots-game-cluster \
            --service game-server \
            --force-new-deployment \
            --desired-count 3
```

### Environment Configuration

```bash
# Production .env
NODE_ENV=production
PORT=3000
WS_PORT=3001

# Redis
REDIS_CLUSTER_ENDPOINTS=redis1:6379,redis2:6379,redis3:6379
REDIS_PASSWORD=${REDIS_PASSWORD}

# PostgreSQL
DATABASE_URL=postgresql://user:pass@db.example.com:5432/robots_game
DB_POOL_SIZE=20

# Security
JWT_SECRET=${JWT_SECRET}
CORS_ORIGINS=https://game.example.com,https://www.example.com

# Monitoring
SENTRY_DSN=${SENTRY_DSN}
PROMETHEUS_PORT=9090

# AWS
AWS_REGION=us-east-1
S3_BUCKET=robots-game-assets
```

---

## Monitoring & Observability

### Metrics to Track

**System Metrics:**
- CPU usage per node
- Memory usage
- Network I/O
- Disk I/O

**Application Metrics:**
- Active WebSocket connections
- Messages per second
- Room count (active/total)
- Average game duration
- Player count by phase
- Vote latency (submission to result)
- Error rate by type

**Business Metrics:**
- Daily/Weekly/Monthly active users
- Average players per room
- Game completion rate
- Most popular game mode
- Question difficulty accuracy

### Alerting Rules

```yaml
# prometheus-alerts.yml
groups:
  - name: game-server
    rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.05
        annotations:
          summary: "Error rate above 5%"

      - alert: HighLatency
        expr: histogram_quantile(0.95, rate(websocket_message_duration_seconds_bucket[5m])) > 0.1
        annotations:
          summary: "95th percentile latency above 100ms"

      - alert: RedisDown
        expr: up{job="redis"} == 0
        annotations:
          summary: "Redis cluster unreachable"
```

### Logging Strategy

```typescript
// Structured logging with Winston
import winston from 'winston';

const logger = winston.createLogger({
    level: 'info',
    format: winston.format.combine(
        winston.format.timestamp(),
        winston.format.json()
    ),
    defaultMeta: { service: 'game-server' },
    transports: [
        new winston.transports.File({ filename: 'error.log', level: 'error' }),
        new winston.transports.File({ filename: 'combined.log' }),
        new winston.transports.Console({
            format: winston.format.simple()
        })
    ]
});

// Usage
logger.info('Player joined room', {
    playerId: player.id,
    roomCode: room.code,
    playerCount: room.players.length
});

logger.error('Vote aggregation failed', {
    roomCode,
    error: error.message,
    stack: error.stack
});
```

---

## Disaster Recovery

### Backup Strategy

**Redis:**
- AOF (Append Only File) enabled
- RDB snapshots every 5 minutes
- Replicate to S3 every hour

**PostgreSQL:**
- Automated daily backups (RDS)
- Point-in-time recovery (35 days)
- Cross-region replication

**Recovery Time Objective (RTO):** 15 minutes
**Recovery Point Objective (RPO):** 5 minutes

### Failover Procedures

1. **Redis Node Failure:**
   - Sentinel promotes replica to master
   - Automatic, <30 seconds downtime
   - Clients reconnect automatically

2. **Game Server Failure:**
   - Load balancer removes unhealthy node
   - Players reconnect to healthy nodes
   - State preserved in Redis

3. **Database Failure:**
   - RDS Multi-AZ automatic failover
   - <2 minutes downtime
   - Application retries with exponential backoff

---

## Cost Estimation (AWS)

**Monthly Costs (Production - 1000 concurrent players):**

| Service | Config | Cost/Month |
|---------|--------|------------|
| ECS Fargate (3 tasks) | 2 vCPU, 4GB each | $180 |
| ElastiCache Redis | 3-node r6g.large | $450 |
| RDS PostgreSQL | db.t3.medium Multi-AZ | $150 |
| ALB | Application Load Balancer | $30 |
| CloudWatch | Logs + Metrics | $50 |
| S3 | Asset storage | $10 |
| Data Transfer | 1TB/month | $90 |
| **Total** | | **~$960/month** |

**Scaling Costs:**
- 10,000 players: ~$3,500/month
- 100,000 players: ~$18,000/month

---

## Development Timeline

### Phase 1: Foundation (Week 1-2)
- [ ] Project setup (TypeScript, Node.js, dependencies)
- [ ] WebSocket server implementation
- [ ] Redis connection and basic operations
- [ ] PostgreSQL schema and migrations
- [ ] Basic message routing

### Phase 2: Core Game Logic (Week 3-4)
- [ ] Room management (create, join, leave)
- [ ] Player session handling
- [ ] Question loading and distribution
- [ ] Answer submission and validation
- [ ] Vote aggregation logic

### Phase 3: Game Flow (Week 5-6)
- [ ] State machine implementation
- [ ] Phase transitions
- [ ] Timer synchronization
- [ ] Scoring calculation
- [ ] Game completion flow

### Phase 4: Polish & Testing (Week 7-8)
- [ ] Error handling and recovery
- [ ] Input validation and sanitization
- [ ] Rate limiting and security
- [ ] Unit tests (80%+ coverage)
- [ ] Integration tests
- [ ] Load testing

### Phase 5: Deployment (Week 9-10)
- [ ] Infrastructure as Code (Terraform)
- [ ] CI/CD pipeline setup
- [ ] Monitoring and logging
- [ ] Staging environment testing
- [ ] Production deployment
- [ ] Documentation

**Total: 10 weeks for complete, production-ready system**

---

## Next Steps

1. **Choose deployment platform:**
   - AWS (recommended - most complete)
   - Google Cloud Platform
   - Microsoft Azure
   - Self-hosted (Kubernetes)

2. **Set up development environment:**
   - Install Node.js 18+, TypeScript
   - Docker + Docker Compose for local Redis/Postgres
   - Unity WebSocket library (NativeWebSocket)

3. **Build MVP first:**
   - Single server (no cluster)
   - 2-player support only
   - Basic room management
   - One full game round
   - **Timeline: 2 weeks**

4. **Scale to production:**
   - Add clustering
   - Implement full security
   - Deploy to cloud
   - **Timeline: 8 additional weeks**

---

## Summary

This architecture provides:

✅ **Scalability:** Handles 1,000+ concurrent games
✅ **Reliability:** 99.9% uptime with automatic failover
✅ **Performance:** <50ms message latency
✅ **Security:** JWT auth, rate limiting, input sanitization
✅ **Maintainability:** Clean architecture, comprehensive monitoring
✅ **Cost-Effective:** Starts at <$1,000/month, scales linearly

The system is designed using industry best practices and proven technologies, ensuring a robust foundation for your multiplayer party game.
