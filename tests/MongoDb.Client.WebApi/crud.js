import http from 'k6/http';
import { check } from 'k6';

export default function () {
    var url = 'http://localhost:5000/api/geoip';
    var geturl = 'http://localhost:5000/api/geoip/601acfaaf1ef1a93fb0c9777';

    var payload = JSON.stringify({
        id: '601acfaaf1ef1a93fb0c9777',
        status: 'aaaaaaaa',
        country: 'bbbbbbbbbb',
        countryCode: 'ccccccccc',
        region: 'dddddddddddd',
        regionName: 'eeeeeeeeeeeee',
        city: 'ffffffffffffff',
        zip: 1111,
        lat: 123,
        lon: 4242,
        timezone: 'gggggggggg',
        isp: 'hhhhhhhhhhhhhhhh',
        org: 'iiiiiiiiiiiiiii',
        query: 'jjjjjjjjjjjj',
    });

    var params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    let res1 = http.post(url, payload, params);
    let res2 = http.get(geturl);
    let res3 = http.del(geturl);
    check(res1, { 'status was 200': (r) => r.status == 200 });
    check(res2, { 'status was 200': (r) => r.status == 200 });
    check(res3, { 'status was 200': (r) => r.status == 200 });
}