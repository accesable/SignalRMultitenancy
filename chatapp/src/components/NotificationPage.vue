<script setup lang="ts">
import { HubConnectionBuilder } from '@microsoft/signalr';
import axios from 'axios';
import { ref, onMounted } from 'vue';
import { v4 as uuidv4 } from 'uuid';

const API_BASE = 'http://localhost:5223';

const notifications = ref<string[]>([]);
const comment = ref<string>('');
const allUsers = ref<string[]>([]);
const selectedUserId = ref<string>('');

const resourceId = uuidv4();
const userId = uuidv4();
console.log(`Connection initialized resource ID: ${resourceId} and user ID: ${userId}`);

const connection = new HubConnectionBuilder()
    .withUrl(`${API_BASE}/hubs/notifications`, {
        headers: {
            "X-Tenant-Id" : `${resourceId}`,
            "X-User-Id" : `${userId}`
        },
    })
    .build();

const fetchUsers = async () => {
    try {
        const res = await axios.get<string[]>(`${API_BASE}/api/v1/users`);
        allUsers.value = res.data;
    } catch (err: any) {
        console.error('Error fetching users:', err.response?.data || err.message);
    }
};

const sendNotification = async (text: string) => {
    if (!selectedUserId.value) {
        alert('Please select a user to send the notification to.');
        return;
    }
    try {
        await axios.post(`${API_BASE}/api/v1/feedback`, {
            resourceId,
            userId: selectedUserId.value,
            comment: text,
        });
        console.log('Notification Sent');
    } catch (err: any) {
        console.error('Error:', err.response?.data || err.message);
    }
};

async function start() {
    try {
        await connection.start();
        console.log('SignalR Connected.');
        await fetchUsers();
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
}

connection.onclose(async () => {
    await start();
});

connection.on('Notification', (text: string) => {
    notifications.value.push(text);
});

onMounted(() => start());
</script>

<template>
    <section id="center">
        <div class="header">
            <h1>Vue JS Notification System</h1>
            <p>Using Notification with SignalR</p>
            <p><strong>Your user ID:</strong> {{ userId }}</p>
        </div>

        <div class="body">
            <div>
                <label>Send to user:</label>
                <button @click="fetchUsers" style="margin-left: 8px;">Refresh Users</button>
                <ul>
                    <li
                        v-for="uid in allUsers"
                        :key="uid"
                        @click="selectedUserId = uid"
                        :style="{ cursor: 'pointer', fontWeight: selectedUserId === uid ? 'bold' : 'normal', background: selectedUserId === uid ? '#d0f0ff' : 'transparent' }"
                    >
                        {{ uid }} {{ uid === userId ? '(you)' : '' }}
                    </li>
                </ul>
                <p v-if="allUsers.length === 0">No users connected yet.</p>
            </div>

            <div>
                <input v-model="comment" placeholder="comment here" />
                <button @click="sendNotification(comment)" :disabled="!selectedUserId">Comment</button>
            </div>
        </div>

        <div>
            <h3>Notifications received:</h3>
            <ul>
                <li v-for="(note, index) in notifications" :key="index">{{ note }}</li>
            </ul>
        </div>
    </section>
</template>
