import http from 'k6/http';
import { check } from 'k6';

export default function () {
    var url = 'http://localhost:5000/api/Geoip';

    let res = http.get(url);
    check(res, { 'status was 200': (r) => r.status == 200 });
}
