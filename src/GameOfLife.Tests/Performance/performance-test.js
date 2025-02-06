import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    stages: [
        { duration: '10s', target: 10 },  // Ramp-up to 10 users
        { duration: '30s', target: 50 },  // Hold at 50 users
        { duration: '10s', target: 0 }    // Ramp-down
    ],
    thresholds: {
        'http_req_duration{status:200}': ['p(95)<100'],  // 95% of requests must be < 100ms
        'http_req_failed': ['rate<0.01'],  // Less than 1% failure rate
        'http_reqs': ['count>1000'],  // Ensure at least 100 requests are processed
    }
};

export default function () {
    let board =
        [
            [0, 1, 0],
            [1, 0, 1],
            [0, 1, 0]
        ];

    let res = http.post('http://localhost:80/api/gameoflife/upload', JSON.stringify(board), {
        headers: { 'Content-Type': 'application/json' }
    });

    check(res, {
        'Status is 201': (r) => r.status === 201,
        'Response time is acceptable': (r) => r.timings.duration < 100
    });

    sleep(1);  // Simulate real-world user wait time
}
