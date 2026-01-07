javascript: (
  followers = [{ id: ``, name: `` }],
  followings = [{ id: ``, name: `` }],
  username = prompt(`※Input your account ID and wait for 5~10s.\nProcess info is available in console.`),

  ToURL = (lst_followers, lst_followings) => {
    let map_merge = new Map();
    lst_followers.forEach(e => map_merge.set(e.id, { name: e.name, type: `r` }));
    lst_followings.forEach(e => map_merge.set(e.id, { name: e.name, type: map_merge.has(e.id) ? `rg` : `g` }));
    const csvContent = `Id,Name,Catagory,Stamp\n`+[...map_merge].map(e => `\%60${e[0]}\%60,\%60${e[1].name}\%60,\%60${e[1].type}\%60,➕`).join(`\n`);
    const blob = new Blob([`\uFEFF` + csvContent], { type: `text/csv;charset=utf-8;` });
    return URL.createObjectURL(blob);
  },
  (async () => {
    try {
      if (window.location.hostname != `www.instagram.com`) throw new Error(`Please do this on www.instagram.com`);
      //const username = prompt(`※按下確定後等待5~10秒，或開啟console確認進度。\n\n請輸入ID:`);
      if (username === null) throw new Error(`Action cancelled.`);
      followers = [];
      followings = [];

      const style_base = {
        position: `fixed`, // 固定位置
        bottom: `20px`, // 距離底部 20 像素
        fontSize: `16px`, // 字體大小
        color: `#FFFFFF`, // 字體顏色 
        border: `none`, // 無邊框
        borderRadius: `5px`, // 圓角
        cursor: `pointer` // 鼠標指針樣式
      };
      const btn_dl = document.body.appendChild(document.createElement(`a`));
      Object.assign(
        Object.assign(btn_dl, {
          innerText: `Download`,
          download: `Data-${new Date().toDateString().split(' ').slice(1).join('-')}.csv`,
        }).
          style,
        {
          ...style_base,
          padding: `10px 20px`, // 添加內邊距
          right: `65px`, // 距離右邊 20 像素
          backgroundColor: `#969696ff`, // 背景顏色(未啟用) => #007BFF
        }
      )
      const btn_close = document.body.appendChild(document.createElement(`button`));
      Object.assign(
        Object.assign(btn_close, {
          innerText: `✕`,
          disabled: false,
          onclick: () => {
            btn_dl.remove();
            btn_close.remove();
            console.log(`UI closed.`)
          }
        }).
          style,
        {
          ...style_base,
          right: `20px`,
          padding: `8px`,
          backgroundColor: `#f87daa`,
          width: `38px`,
          height: `38px`,
          display: `flex`,
          alignItems: `center`,
          justifyContent: `center`,
          fontWeight: 700,
        }
      )
      console.log(`Process started! Give it a couple of seconds`);
      const userQueryRes = await fetch(`https://www.instagram.com/web/search/topsearch/?query=${username}`);
      const userQueryJson = await userQueryRes.json();
      const userId = userQueryJson.users.map(u => u.user).filter(u => u.username === username)[0].pk;
      let after = null;
      let has_next = true;
      console.log(`Fetching follower list...`);
      while (has_next) {
        await fetch(`https://www.instagram.com/graphql/query/?query_hash=c76146de99bb02f6415203be841dd25a&variables=` +
          encodeURIComponent(JSON.stringify({
            id: userId,
            include_reel: true,
            fetch_mutual: true,
            first: 50,
            after: after,
          }))
        )
          .then((res) => res.json())
          .then((res) => {
            has_next = res.data.user.edge_followed_by.page_info.has_next_page;
            after = res.data.user.edge_followed_by.page_info.end_cursor;
            followers = followers.concat(res.data.user.edge_followed_by.edges.map(({ node }) => {
              return { id: node.username, name: node.full_name, };
            }));
          });
      }
      console.log(`Done.`);
      after = null;
      has_next = true;
      console.log(`Fetching following list...`);
      while (has_next) {
        await fetch(`https://www.instagram.com/graphql/query/?query_hash=d04b0a864b4b54837c0d870b0e77e076&variables=` +
          encodeURIComponent(JSON.stringify({
            id: userId, include_reel: true,
            fetch_mutual: true,
            first: 50,
            after: after,
          }))
        )
          .then((res) => res.json())
          .then((res) => {
            has_next = res.data.user.edge_follow.page_info.has_next_page;
            after = res.data.user.edge_follow.page_info.end_cursor;
            followings = followings.concat(
              res.data.user.edge_follow.edges.map(({ node }) => {
                return {
                  id: node.username,
                  name: node.full_name,
                };
              })
            );
          });
      }
      btn_dl.setAttribute(`href`, ToURL(followers, followings))
      btn_dl.style.backgroundColor = `#007BFF`;
      console.log(`Done.\nProcess ended.`);
    } catch (error) {
      alert(error);
      console.log(`Process stopped by error.`);
      return `0`;
    }
  })()
)