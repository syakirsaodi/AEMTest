




SELECT p.uniqueName as PlatformName , w.id ,p.id as PlatformId, w.uniqueName, w.latitude,w.longitude,w.createdAt, w.updatedAt
FROM Platform p INNER JOIN
     (select w.*, row_number() over (partition by platformId order by updatedAt desc) as seqnum
      from Well w
     ) w
    ON p.id = w.platformId and seqnum = 1
ORDER BY p.id;