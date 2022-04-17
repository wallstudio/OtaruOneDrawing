# Design

```
GASTrigger
↓
GAS(Cron MakiOneDrawing) → GASTrigger(schedule next)
↓
GithubWorkflow
  ⇄ SpreadSheet(DB MakiOneDrawing)
  ⇄ TwitterAPI(@makimaki_draw)
```

# Commands

```
cmd /c "git checkout master && git pull && git merge develop && git push && git checkout develop"
```
# Links

- Google
  - [GCPConsole](https://console.cloud.google.com/iam-admin/serviceaccounts/details/116370453942115831918;edit=true?previousPage=%2Fapis%2Fcredentials%3Fauthuser%3D1%26project%3Dmakionedrawingbot&authuser=1&project=makionedrawingbot)
  - [GASProjectsHome](https://script.google.com/u/1/home)
  - [DB](https://docs.google.com/spreadsheets/d/1WBH5ZUl8dx24gWDg7dnVjDUGacUNuZX2rtilJUegUdI/edit#gid=1297766856)
  - [Cron](https://script.google.com/u/1/home/projects/1KkhWQBIWylJvgWIZAi-XM_i3vdexD6QiJZ8fY5Kelk66cqf00dZPlBnX/edit)
  - [Calendar](https://calendar.google.com/calendar/u/1?cid=MjIzMjZtYmcxY2JkODhiaTVzN2U5Y2tsYmdAZ3JvdXAuY2FsZW5kYXIuZ29vZ2xlLmNvbQ)
- Github
  - [GithubAPIToken](https://github.com/settings/tokens)
  - [GithubActions](https://github.com/wallstudio/MakiOneDrawing/actions)
- Twitter
  - [TweetDeck](https://tweetdeck.twitter.com/)
  - [TwitterDeveloperPortal](https://developer.twitter.com/en/portal/dashboard)

# 新しいワンドロを作る

## 1. Twitterアカウントの作成
DeveloperPortalも一緒に、Elevatedも  
https://developer.twitter.com/en/portal/products/elevated

## 2. マキドロをフォーク
https://github.com/wallstudio/MakiOneDrawing

## 3. masterを`1e668a0`までリセット

## 4. GAS->GithubAction シークレット
Githubのユーザページで、GithubActionにアクセスするためのPersonalKeyTokenを作成して、`gas/Secret.ts` に `getGithubToken():string`で実装する  
https://github.com/settings/tokens/

## 5. GithubAction->Twitter シークレット
GithubにTwitterシークレット情報（5つ）を登録  
https://developer.twitter.com/en/portal/projects/1513578980787769344/apps/23956234/keys

## 6. GithubAction->Google シークレット
GithubにGooglePlatformのサービスアカウントのJWTをbased64で登録  
`b64.exe`の標準入力にPathを食わせると相互変換可能  
https://console.cloud.google.com/iam-admin/serviceaccounts?authuser=1&project=makionedrawingbot

## 7. DBとなるシートを作成
https://docs.google.com/spreadsheets/d/1sKepPzYyauMfVQ9ZOZqvTok3yXa3CngDrNShk7Ib15Q/edit?usp=sharing

## 8. コード修正
`docs/img/` の画像を差し替え  
`View.cs` の文章を修正  
`Actions.cs` のシートIDを修正(`Actions.DB_SHEET_ID`)
