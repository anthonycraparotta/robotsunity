const WebSocket = require('ws');
const { v4: uuidv4 } = require('uuid');

const PORT = process.env.PORT || 3000;

// In-memory storage for rooms and players
const rooms = {};

// Create WebSocket server
const wss = new WebSocket.Server({ port: PORT });

console.log(`WebSocket server running on port ${PORT}`);

wss.on('connection', (ws) => {
  ws.id = uuidv4();
  console.log(`Client connected: ${ws.id}`);

  ws.on('message', (message) => {
    try {
      const data = JSON.parse(message);
      handleMessage(ws, data);
    } catch (error) {
      console.error('Error parsing message:', error);
      sendError(ws, 'invalid-message-format');
    }
  });

  ws.on('close', () => {
    console.log(`Client disconnected: ${ws.id}`);
    handleDisconnect(ws);
  });

  ws.on('error', (error) => {
    console.error(`WebSocket error for ${ws.id}:`, error);
  });
});

function handleMessage(ws, data) {
  const { action } = data;

  console.log(`Received action: ${action} from ${ws.id}`);

  switch (action) {
    case 'create-room':
      handleCreateRoom(ws, data);
      break;
    case 'join-room':
      handleJoinRoom(ws, data);
      break;
    case 'start-game':
      handleStartGame(ws, data);
      break;
    case 'submit-answer':
      handleSubmitAnswer(ws, data);
      break;
    case 'submit-elimination-vote':
      handleSubmitEliminationVote(ws, data);
      break;
    case 'submit-final-vote':
      handleSubmitFinalVote(ws, data);
      break;
    case 'broadcast-round-scores':
      handleBroadcastRoundScores(ws, data);
      break;
    default:
      console.warn(`Unknown action: ${action}`);
      sendError(ws, 'unknown-action');
  }
}

function handleCreateRoom(ws, data) {
  const { playerName, playerIcon } = data;
  const roomCode = generateRoomCode();

  rooms[roomCode] = {
    code: roomCode,
    host: ws.id,
    players: [
      {
        id: ws.id,
        name: playerName,
        icon: playerIcon,
        score: 0
      }
    ],
    gameStarted: false,
    currentRound: 0
  };

  ws.roomCode = roomCode;
  ws.playerName = playerName;
  ws.isHost = true;

  sendToClient(ws, {
    type: 'room-joined',
    roomCode: roomCode,
    data: JSON.stringify({ roomCode, isHost: true })
  });

  broadcastPlayersUpdate(roomCode);

  console.log(`Room created: ${roomCode} by ${playerName}`);
}

function handleJoinRoom(ws, data) {
  const { roomCode, playerName, playerIcon } = data;
  const room = rooms[roomCode];

  if (!room) {
    sendError(ws, 'room-not-found');
    return;
  }

  if (room.gameStarted) {
    sendError(ws, 'game-already-started');
    return;
  }

  // Check if player name already exists
  if (room.players.find(p => p.name === playerName)) {
    sendError(ws, 'player-name-taken');
    return;
  }

  room.players.push({
    id: ws.id,
    name: playerName,
    icon: playerIcon,
    score: 0
  });

  ws.roomCode = roomCode;
  ws.playerName = playerName;
  ws.isHost = false;

  sendToClient(ws, {
    type: 'room-joined',
    roomCode: roomCode,
    data: JSON.stringify({ roomCode, isHost: false })
  });

  broadcastToRoom(roomCode, {
    type: 'player-joined',
    data: JSON.stringify({ playerName, playerIcon })
  });

  broadcastPlayersUpdate(roomCode);

  console.log(`${playerName} joined room ${roomCode}`);
}

function handleStartGame(ws, data) {
  const { roomCode, roundCount } = data;
  const room = rooms[roomCode];

  if (!room) {
    sendError(ws, 'room-not-found');
    return;
  }

  if (room.host !== ws.id) {
    sendError(ws, 'not-authorized');
    return;
  }

  room.gameStarted = true;
  room.totalRounds = roundCount || 8;

  broadcastToRoom(roomCode, {
    type: 'game-started',
    data: JSON.stringify({ totalRounds: room.totalRounds })
  });

  console.log(`Game started in room ${roomCode} with ${roundCount} rounds`);
}

function handleSubmitAnswer(ws, data) {
  const { roomCode, playerName, answer } = data;
  const room = rooms[roomCode];

  if (!room) {
    sendError(ws, 'room-not-found');
    return;
  }

  if (!room.answers) {
    room.answers = {};
  }

  room.answers[playerName] = answer;

  // Notify room that player answered
  broadcastToRoom(roomCode, {
    type: 'player-answered',
    data: JSON.stringify({ playerName })
  });

  // Check if all players answered
  if (Object.keys(room.answers).length >= room.players.length) {
    // Compile all answers
    const allAnswers = Object.entries(room.answers).map(([name, text]) => ({
      text,
      type: 'player',
      playerName: name
    }));

    broadcastToRoom(roomCode, {
      type: 'all-answers-submitted',
      data: JSON.stringify({ answers: allAnswers })
    });

    console.log(`All answers submitted in room ${roomCode}`);
  }
}

function handleSubmitEliminationVote(ws, data) {
  const { roomCode, playerName, vote } = data;
  const room = rooms[roomCode];

  if (!room) {
    sendError(ws, 'room-not-found');
    return;
  }

  if (!room.eliminationVotes) {
    room.eliminationVotes = {};
  }

  room.eliminationVotes[playerName] = vote;

  // Notify room that vote was cast
  broadcastToRoom(roomCode, {
    type: 'elimination-vote-cast',
    data: JSON.stringify({ playerName, vote })
  });

  // Check if all players voted
  if (Object.keys(room.eliminationVotes).length >= room.players.length) {
    // Calculate elimination results
    const voteCounts = {};
    Object.values(room.eliminationVotes).forEach(v => {
      voteCounts[v] = (voteCounts[v] || 0) + 1;
    });

    const maxVotes = Math.max(...Object.values(voteCounts));
    const mostVoted = Object.keys(voteCounts).filter(a => voteCounts[a] === maxVotes);

    const tieOccurred = mostVoted.length > 1;
    const eliminatedAnswer = tieOccurred ? null : mostVoted[0];

    broadcastToRoom(roomCode, {
      type: 'elimination-complete',
      data: JSON.stringify({
        eliminatedAnswer,
        tieOccurred,
        voteCounts
      })
    });

    console.log(`Elimination complete in room ${roomCode}: ${eliminatedAnswer || 'TIE'}`);
  }
}

function handleSubmitFinalVote(ws, data) {
  const { roomCode, playerName, vote } = data;
  const room = rooms[roomCode];

  if (!room) {
    sendError(ws, 'room-not-found');
    return;
  }

  if (!room.finalVotes) {
    room.finalVotes = {};
  }

  room.finalVotes[playerName] = vote;

  // Notify room that vote was cast
  broadcastToRoom(roomCode, {
    type: 'final-vote-cast',
    data: JSON.stringify({ playerName, vote })
  });

  // Check if all players voted
  if (Object.keys(room.finalVotes).length >= room.players.length) {
    broadcastToRoom(roomCode, {
      type: 'all-votes-submitted',
      data: JSON.stringify({
        votes: Object.entries(room.finalVotes).map(([playerName, vote]) => ({
          playerName,
          answer: vote
        }))
      })
    });

    console.log(`All final votes submitted in room ${roomCode}`);
  }
}

function handleBroadcastRoundScores(ws, data) {
  const { roomCode, roundNumber, playerScores } = data;
  const room = rooms[roomCode];

  console.log(`Received broadcast-round-scores for room ${roomCode}, round ${roundNumber}`);

  if (!roomCode || typeof roomCode !== 'string') {
    console.warn('broadcast-round-scores: missing room code');
    sendError(ws, 'room-code-required');
    return;
  }

  if (!room) {
    console.warn(`broadcast-round-scores: room ${roomCode} not found`);
    sendError(ws, 'room-not-found');
    return;
  }

  // Verify sender is the host
  if (room.host !== ws.id) {
    console.warn(`Unauthorized broadcast-round-scores attempt by ${ws.id} in room ${roomCode}`);
    sendError(ws, 'not-authorized');
    return;
  }

  if (!Array.isArray(playerScores)) {
    console.warn('broadcast-round-scores: playerScores must be an array');
    sendError(ws, 'invalid-player-scores');
    return;
  }

  // Apply scores to server's player objects
  playerScores.forEach(scoreData => {
    const player = room.players.find(p => p.name === scoreData.name);
    if (player) {
      player.score = scoreData.score;
      console.log(`Updated ${scoreData.name} score to ${scoreData.score}`);
    }
  });

  // Broadcast final-round-scores event to all clients with updated scores
  broadcastToRoom(roomCode, {
    type: 'final-round-scores',
    data: JSON.stringify({
      roundNumber: roundNumber,
      playerScores: playerScores,
      players: room.players // Include full player objects for compatibility
    })
  });

  console.log(`Broadcasted final-round-scores for room ${roomCode}, round ${roundNumber}`);

  // Clear votes for next round
  room.answers = {};
  room.eliminationVotes = {};
  room.finalVotes = {};
}

function handleDisconnect(ws) {
  if (!ws.roomCode) return;

  const room = rooms[ws.roomCode];
  if (!room) return;

  // Remove player from room
  room.players = room.players.filter(p => p.id !== ws.id);

  if (room.players.length === 0) {
    // Delete room if empty
    delete rooms[ws.roomCode];
    console.log(`Room ${ws.roomCode} deleted (empty)`);
  } else {
    // Notify remaining players
    broadcastToRoom(ws.roomCode, {
      type: 'player-left',
      data: JSON.stringify({ playerName: ws.playerName })
    });

    // If host left, assign new host
    if (room.host === ws.id) {
      room.host = room.players[0].id;
      console.log(`New host assigned in room ${ws.roomCode}: ${room.players[0].name}`);
    }

    broadcastPlayersUpdate(ws.roomCode);
  }
}

function broadcastToRoom(roomCode, message) {
  const room = rooms[roomCode];
  if (!room) return;

  const playerIds = room.players.map(p => p.id);

  wss.clients.forEach(client => {
    if (client.readyState === WebSocket.OPEN && playerIds.includes(client.id)) {
      client.send(JSON.stringify(message));
    }
  });
}

function broadcastPlayersUpdate(roomCode) {
  const room = rooms[roomCode];
  if (!room) return;

  broadcastToRoom(roomCode, {
    type: 'players-update',
    data: JSON.stringify({
      players: room.players.map(p => ({
        name: p.name,
        icon: p.icon,
        score: p.score
      }))
    })
  });
}

function sendToClient(ws, message) {
  if (ws.readyState === WebSocket.OPEN) {
    ws.send(JSON.stringify(message));
  }
}

function sendError(ws, errorCode) {
  sendToClient(ws, {
    type: 'error',
    data: JSON.stringify({ error: errorCode })
  });
}

function generateRoomCode() {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
  let code;
  do {
    code = Array.from({ length: 4 }, () => chars[Math.floor(Math.random() * chars.length)]).join('');
  } while (rooms[code]);
  return code;
}

// Graceful shutdown
process.on('SIGTERM', () => {
  console.log('SIGTERM received, closing server...');
  wss.close(() => {
    console.log('Server closed');
    process.exit(0);
  });
});
