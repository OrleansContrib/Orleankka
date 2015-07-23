
type HelloMessage =
   | Hi of string
   | WhatIsYourName
   | GiveMeMoney of currency:string * amount:double

in order to start, you need to send a request:

1)  [POST] http://localhost:48213/api/TestActor/http_test/Hi

	Content-Type: orleankka/vnd.actor+json

	Body:
	"'Super Man'"


2)  [POST] http://localhost:48213/api/TestActor/http_test/WhatIsYourName

	Content-Type: orleankka/vnd.actor+json

	Body:


3)  [POST] http://localhost:48213/api/TestActor/http_test/GiveMeMoney

    Content-Type: orleankka/vnd.actor+json

	Body:
	"'USD', '4.5'"