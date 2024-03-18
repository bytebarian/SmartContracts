var express = require('express')
var app = express()

app.get('/', function (req, res) {
  res.send("558")
})

app.listen(3000)