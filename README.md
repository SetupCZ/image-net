# ImageNET

## App

### SourceDataProcessor 
is a console app that processes the data from the source and saves it to the database.

### App
is a web app that renders the data in a tree structure.

- http://localhost:5216/root renders just a root node with its immediate children.
- http://localhost:5216/ renders the whole tree. !! Warning: it may take a while to load !!
- http://localhost:5216/{node_id} renders the node by its id and its immediate children. It's also used to expand the tree.

## Database migration

```bash
# Init migration
$ dotnet ef database update --project Library 
```
